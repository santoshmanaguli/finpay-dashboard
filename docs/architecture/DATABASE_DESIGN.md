# Database Design Documentation

## Overview

The FinPay Dashboard uses a relational database design optimized for financial data management, transaction processing, and user management. The database is designed with security, performance, and scalability in mind.

## Database Schema

### Entity Relationship Diagram

```
┌─────────────────┐    1:N    ┌─────────────────┐    1:N    ┌─────────────────┐
│     Users       │◄─────────►│   CreditCards   │◄─────────►│  Transactions   │
│                 │           │                 │           │                 │
│ Id (PK)         │           │ Id (PK)         │           │ Id (PK)         │
│ Email (UQ)      │           │ UserId (FK)     │           │ CardId (FK)     │
│ PasswordHash    │           │ CardNumberLast4 │           │ Amount          │
│ FirstName       │           │ CardHolderName  │           │ Description     │
│ LastName        │           │ ExpiryDate      │           │ CategoryId (FK) │
│ PhoneNumber     │           │ CardType        │           │ MerchantName    │
│ IsActive        │           │ CreditLimit     │           │ TransactionDate │
│ CreatedAt       │           │ AvailableBalance│           │ Status          │
│ UpdatedAt       │           │ CurrentBalance  │           │ TransactionType │
└─────────────────┘           │ IsActive        │           │ CreatedAt       │
       │                      │ CreatedAt       │           └─────────────────┘
       │ 1:N                  │ UpdatedAt       │                    │
       │                      └─────────────────┘                    │ N:1
       │                                                             │
       │                                                             ▼
       │                                                    ┌─────────────────┐
       │                                                    │   Categories    │
       │                                                    │                 │
       │                                                    │ Id (PK)         │
       │                                                    │ Name (UQ)       │
       │                                                    │ Description     │
       │                                                    │ IconUrl         │
       │                                                    │ Color           │
       │                                                    │ IsActive        │
       │                                                    │ CreatedAt       │
       │                                                    └─────────────────┘
       │ 1:N                                                         ▲
       │                                                             │ N:1
       ▼                                                             │
┌─────────────────┐                                         ┌─────────────────┐
│    Rewards      │                                         │  Transactions   │
│                 │                                         │   (Reference)   │
│ Id (PK)         │                                         └─────────────────┘
│ UserId (FK)     │
│ TransactionId   │
│ PointsEarned    │
│ PointsRedeemed  │
│ RewardType      │
│ Description     │
│ EarnedDate      │
│ RedeemedDate    │
└─────────────────┘
```

## Table Specifications

### 1. Users Table

```sql
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PhoneNumber NVARCHAR(15) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    INDEX IX_Users_Email (Email),
    INDEX IX_Users_IsActive (IsActive),
    INDEX IX_Users_CreatedAt (CreatedAt)
);
```

**Purpose**: Store user account information and authentication data
**Key Features**:
- GUID primary key for security
- Unique email constraint
- Password stored as BCrypt hash
- Soft delete using IsActive flag
- Audit trail with timestamps

### 2. CreditCards Table

```sql
CREATE TABLE CreditCards (
    Id NVARCHAR(450) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    CardNumberLastFour NVARCHAR(4) NOT NULL,
    CardHolderName NVARCHAR(100) NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    CardType NVARCHAR(20) NOT NULL,
    CreditLimit DECIMAL(18,2) NOT NULL,
    AvailableBalance DECIMAL(18,2) NOT NULL,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_CreditCards_UserId (UserId),
    INDEX IX_CreditCards_IsActive (IsActive),
    INDEX IX_CreditCards_ExpiryDate (ExpiryDate)
);
```

**Purpose**: Store credit card information for each user
**Security Features**:
- Only last 4 digits stored (PCI DSS compliance)
- No full card numbers or CVV stored
- User isolation through foreign key

### 3. Categories Table

```sql
CREATE TABLE Categories (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    IconUrl NVARCHAR(100) NULL,
    Color NVARCHAR(7) NOT NULL DEFAULT '#000000',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    INDEX IX_Categories_Name (Name),
    INDEX IX_Categories_IsActive (IsActive)
);
```

**Purpose**: Define transaction categories for classification
**Features**:
- Pre-seeded with common categories
- Support for custom icons and colors
- Extensible for user-defined categories

### 4. Transactions Table

```sql
CREATE TABLE Transactions (
    Id NVARCHAR(450) PRIMARY KEY,
    CardId NVARCHAR(450) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    CategoryId NVARCHAR(450) NOT NULL,
    MerchantName NVARCHAR(100) NULL,
    TransactionDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Completed',
    TransactionType NVARCHAR(50) NOT NULL DEFAULT 'Purchase',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (CardId) REFERENCES CreditCards(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT,
    INDEX IX_Transactions_CardId (CardId),
    INDEX IX_Transactions_CategoryId (CategoryId),
    INDEX IX_Transactions_TransactionDate (TransactionDate DESC),
    INDEX IX_Transactions_Amount (Amount),
    INDEX IX_Transactions_Status (Status),
    INDEX IX_Transactions_Composite (CardId, TransactionDate DESC, CategoryId)
);
```

**Purpose**: Store all transaction data
**Performance Features**:
- Composite index for common query patterns
- Optimized for date range queries
- Foreign key constraints for data integrity

### 5. Rewards Table

```sql
CREATE TABLE Rewards (
    Id NVARCHAR(450) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    TransactionId NVARCHAR(450) NULL,
    PointsEarned INT NOT NULL DEFAULT 0,
    PointsRedeemed INT NOT NULL DEFAULT 0,
    RewardType NVARCHAR(100) NOT NULL DEFAULT 'Purchase',
    Description NVARCHAR(200) NULL,
    EarnedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RedeemedDate DATETIME2 NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TransactionId) REFERENCES Transactions(Id) ON DELETE SET NULL,
    INDEX IX_Rewards_UserId (UserId),
    INDEX IX_Rewards_TransactionId (TransactionId),
    INDEX IX_Rewards_EarnedDate (EarnedDate DESC),
    INDEX IX_Rewards_RewardType (RewardType)
);
```

**Purpose**: Track reward points earned and redeemed
**Features**:
- Links to transactions for point calculation
- Supports various reward types
- Tracks redemption history

## Data Types and Constraints

### Decimal Precision
All monetary amounts use `DECIMAL(18,2)` for:
- Precise currency calculations
- Avoiding floating-point errors
- Supporting large transaction amounts (up to $999,999,999,999.99)

### String Lengths
- **IDs**: NVARCHAR(450) - GUID string representation
- **Emails**: NVARCHAR(100) - Standard email length
- **Names**: NVARCHAR(50) - Adequate for most names
- **Descriptions**: NVARCHAR(200) - Brief descriptions
- **Phone**: NVARCHAR(15) - International phone numbers

### Date Handling
- **DATETIME2**: Used for all timestamps (higher precision than DATETIME)
- **UTC Storage**: All dates stored in UTC
- **Default Values**: GETUTCDATE() for audit fields

## Indexes and Performance

### Primary Indexes
All tables use clustered indexes on the primary key (Id column).

### Secondary Indexes

#### Users Table
```sql
-- Email lookup (login)
CREATE INDEX IX_Users_Email ON Users(Email);

-- Active user filtering
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- User creation analytics
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);
```

#### Transactions Table
```sql
-- User transaction queries
CREATE INDEX IX_Transactions_CardId ON Transactions(CardId);

-- Category analytics
CREATE INDEX IX_Transactions_CategoryId ON Transactions(CategoryId);

-- Date range queries (most common)
CREATE INDEX IX_Transactions_TransactionDate ON Transactions(TransactionDate DESC);

-- Amount-based queries and sorting
CREATE INDEX IX_Transactions_Amount ON Transactions(Amount);

-- Status filtering
CREATE INDEX IX_Transactions_Status ON Transactions(Status);

-- Composite index for dashboard queries
CREATE INDEX IX_Transactions_Composite ON Transactions(CardId, TransactionDate DESC, CategoryId);
```

### Query Optimization Examples

#### Common Query Patterns
```sql
-- User's recent transactions (optimized with composite index)
SELECT t.*, c.Name as CategoryName
FROM Transactions t
INNER JOIN Categories c ON t.CategoryId = c.Id
WHERE t.CardId = @CardId
ORDER BY t.TransactionDate DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

-- Monthly spending by category (optimized with indexes)
SELECT
    c.Name,
    SUM(t.Amount) as TotalSpent,
    COUNT(*) as TransactionCount
FROM Transactions t
INNER JOIN Categories c ON t.CategoryId = c.Id
WHERE t.CardId = @CardId
    AND t.TransactionDate >= @StartDate
    AND t.TransactionDate < @EndDate
GROUP BY c.Id, c.Name
ORDER BY TotalSpent DESC;
```

## Security Considerations

### Data Protection
1. **Password Security**
   - Passwords hashed using BCrypt (cost factor 12)
   - No plain text passwords stored
   - Password history not maintained (privacy)

2. **Credit Card Security**
   - Only last 4 digits stored
   - No full PAN, CVV, or PIN data
   - Compliant with PCI DSS Level 1

3. **Data Isolation**
   - Users can only access their own data
   - Foreign key constraints enforce relationships
   - Soft deletes preserve referential integrity

### Access Control
```sql
-- Row-level security example (future enhancement)
CREATE FUNCTION dbo.fn_securitypredicate_user(@UserId NVARCHAR(450))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_securitypredicate_result
WHERE @UserId = USER_NAME() OR IS_MEMBER('db_admin') = 1;

-- Apply to sensitive tables
CREATE SECURITY POLICY UserDataPolicy
ADD FILTER PREDICATE dbo.fn_securitypredicate_user(UserId) ON Transactions,
ADD FILTER PREDICATE dbo.fn_securitypredicate_user(UserId) ON CreditCards,
ADD FILTER PREDICATE dbo.fn_securitypredicate_user(UserId) ON Rewards;
```

## Data Seeding

### Initial Categories
```sql
INSERT INTO Categories (Id, Name, Description, IconUrl, Color) VALUES
('cat-1', 'Food & Dining', 'Restaurants, cafes, and food delivery', '🍽️', '#FF6B6B'),
('cat-2', 'Transportation', 'Gas, public transport, rideshares', '🚗', '#4ECDC4'),
('cat-3', 'Shopping', 'Retail, online shopping, clothing', '🛍️', '#45B7D1'),
('cat-4', 'Entertainment', 'Movies, games, subscriptions', '🎬', '#96CEB4'),
('cat-5', 'Bills & Utilities', 'Electricity, water, internet, phone', '📄', '#FFEAA7'),
('cat-6', 'Healthcare', 'Medical, dental, pharmacy', '⚕️', '#FD79A8'),
('cat-7', 'Travel', 'Hotels, flights, vacation expenses', '✈️', '#FDCB6E'),
('cat-8', 'Education', 'Courses, books, training', '📚', '#6C5CE7'),
('cat-9', 'Fitness', 'Gym, sports, health activities', '💪', '#A29BFE'),
('cat-10', 'Other', 'Miscellaneous expenses', '📦', '#636E72');
```

## Backup and Recovery

### Backup Strategy
1. **Automated Daily Backups** (Azure SQL Database)
   - Point-in-time restore (7 days)
   - Geo-redundant backup storage
   - Transaction log backups every 5-10 minutes

2. **Long-term Retention**
   - Weekly backups retained for 12 weeks
   - Monthly backups retained for 12 months
   - Yearly backups retained for 10 years

### Recovery Scenarios
```sql
-- Point-in-time restore example
RESTORE DATABASE FinPayDB_Restored
FROM DATABASE FinPayDB
WITH REPLACE,
    RESTORE_DATE = '2024-01-15 14:30:00';

-- Geo-restore from backup
RESTORE DATABASE FinPayDB_GeoRestore
FROM GEO_BACKUP = 'FinPayDB_Backup_2024_01_15.bacpac';
```

## Monitoring and Maintenance

### Performance Monitoring
```sql
-- Index usage statistics
SELECT
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID();

-- Query performance insights
SELECT
    qs.execution_count,
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    qt.text AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY avg_elapsed_time DESC;
```

### Maintenance Tasks
1. **Index Maintenance**
   - Weekly index reorganization
   - Monthly index rebuilds for fragmented indexes
   - Statistics updates

2. **Data Cleanup**
   - Archive old transactions (>2 years)
   - Clean up expired sessions
   - Remove inactive user accounts (after notice period)

## Scaling Considerations

### Partitioning Strategy
```sql
-- Partition transactions by date for better performance
CREATE PARTITION FUNCTION pf_TransactionDate (DATETIME2)
AS RANGE RIGHT FOR VALUES
('2024-01-01', '2024-02-01', '2024-03-01', '2024-04-01');

CREATE PARTITION SCHEME ps_TransactionDate
AS PARTITION pf_TransactionDate
ALL TO ([PRIMARY]);

-- Apply partitioning to Transactions table
CREATE TABLE Transactions_Partitioned (
    -- Same columns as Transactions
) ON ps_TransactionDate (TransactionDate);
```

### Read Replicas
For high-read scenarios:
- Use Azure SQL Database read replicas
- Route analytics queries to read replicas
- Maintain write operations on primary database

This database design provides a solid foundation for the FinPay Dashboard, ensuring data integrity, security, and performance while supporting future growth and feature additions.
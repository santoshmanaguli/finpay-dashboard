# API Design Documentation

## Overview

The FinPay Dashboard API is built using ASP.NET Core Web API following RESTful principles. The API provides secure endpoints for authentication, transaction management, dashboard analytics, and user profile management.

## API Architecture

### Design Principles

1. **RESTful Design**: Standard HTTP methods and status codes
2. **Stateless**: Each request contains all necessary information
3. **Consistent**: Uniform response formats and naming conventions
4. **Secure**: JWT authentication and input validation
5. **Versioned**: API versioning for backward compatibility
6. **Documented**: Comprehensive OpenAPI/Swagger documentation

### Base URL Structure

```
Development: https://localhost:7001/api
Production:  https://api.finpay.example.com/api
```

## Authentication & Authorization

### JWT Token Authentication

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Structure
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id-guid",
    "email": "user@example.com",
    "given_name": "John",
    "family_name": "Doe",
    "jti": "token-id",
    "iat": 1640995200,
    "exp": 1640998800,
    "iss": "FinPayDashboard.Api",
    "aud": "FinPayDashboard.Client"
  }
}
```

### Security Headers
```http
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

## API Endpoints

### 1. Authentication Endpoints

#### POST /api/auth/register
Register a new user account.

**Request:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T15:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "+1234567890",
      "createdAt": "2024-01-15T14:30:00Z"
    }
  },
  "message": "User registered successfully"
}
```

#### POST /api/auth/login
Authenticate user and return JWT token.

**Request:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T15:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "+1234567890",
      "createdAt": "2024-01-15T14:30:00Z"
    }
  },
  "message": "Login successful"
}
```

#### GET /api/auth/me
Get current authenticated user information.

**Headers:**
```http
Authorization: Bearer {jwt-token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "createdAt": "2024-01-15T14:30:00Z"
  },
  "message": "User data retrieved successfully"
}
```

### 2. Dashboard Endpoints

#### GET /api/dashboard/summary
Get dashboard summary with key metrics.

**Headers:**
```http
Authorization: Bearer {jwt-token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "currentBalance": 2500.75,
    "availableCredit": 7499.25,
    "creditLimit": 10000.00,
    "monthlySpent": 1245.30,
    "lastMonthSpent": 1108.45,
    "spendingChangePercent": 12.37,
    "upcomingBillDate": "2024-02-15T00:00:00Z",
    "upcomingBillAmount": 2500.75,
    "rewardPoints": 12547,
    "rewardsCashValue": 125.47,
    "transactionCount": 45,
    "lastTransactionDate": "2024-01-14T18:30:00Z"
  },
  "message": "Dashboard summary retrieved successfully"
}
```

#### GET /api/dashboard/spending-trends
Get spending trends for charts and analytics.

**Query Parameters:**
- `period` (string): "week", "month", "quarter", "year"
- `startDate` (string): ISO date string (optional)
- `endDate` (string): ISO date string (optional)

**Example Request:**
```http
GET /api/dashboard/spending-trends?period=month&startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer {jwt-token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "period": "month",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "totalSpent": 1245.30,
    "dailySpending": [
      {
        "date": "2024-01-01",
        "amount": 45.30,
        "transactionCount": 3
      },
      {
        "date": "2024-01-02",
        "amount": 78.95,
        "transactionCount": 5
      }
    ],
    "categoryBreakdown": [
      {
        "categoryId": "cat-1",
        "categoryName": "Food & Dining",
        "amount": 456.78,
        "percentage": 36.7,
        "transactionCount": 18,
        "color": "#FF6B6B"
      },
      {
        "categoryId": "cat-2",
        "categoryName": "Transportation",
        "amount": 234.52,
        "percentage": 18.8,
        "transactionCount": 8,
        "color": "#4ECDC4"
      }
    ]
  },
  "message": "Spending trends retrieved successfully"
}
```

### 3. Transaction Endpoints

#### GET /api/transactions
Get paginated list of transactions with filtering and sorting.

**Query Parameters:**
- `page` (number): Page number (default: 1)
- `pageSize` (number): Items per page (default: 20, max: 100)
- `startDate` (string): Filter by start date (ISO format)
- `endDate` (string): Filter by end date (ISO format)
- `categoryId` (string): Filter by category ID
- `merchantName` (string): Filter by merchant name (partial match)
- `minAmount` (number): Filter by minimum amount
- `maxAmount` (number): Filter by maximum amount
- `status` (string): Filter by status ("Completed", "Pending", "Failed")
- `transactionType` (string): Filter by type ("Purchase", "Refund", "Payment")
- `sortBy` (string): Sort field ("date", "amount", "merchant")
- `sortDirection` (string): Sort direction ("asc", "desc")

**Example Request:**
```http
GET /api/transactions?page=1&pageSize=20&categoryId=cat-1&sortBy=date&sortDirection=desc
Authorization: Bearer {jwt-token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "transactions": [
      {
        "id": "txn-123",
        "amount": 45.30,
        "description": "Coffee and pastry",
        "merchantName": "Starbucks",
        "category": {
          "id": "cat-1",
          "name": "Food & Dining",
          "color": "#FF6B6B",
          "iconUrl": "🍽️"
        },
        "transactionDate": "2024-01-14T18:30:00Z",
        "status": "Completed",
        "transactionType": "Purchase",
        "rewardPoints": 45,
        "createdAt": "2024-01-14T18:30:15Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalCount": 156,
      "totalPages": 8,
      "hasNextPage": true,
      "hasPreviousPage": false
    },
    "summary": {
      "totalAmount": 1245.30,
      "transactionCount": 156,
      "averageAmount": 7.98
    }
  },
  "message": "Transactions retrieved successfully"
}
```

#### GET /api/transactions/{id}
Get specific transaction details.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "txn-123",
    "amount": 45.30,
    "description": "Coffee and pastry",
    "merchantName": "Starbucks",
    "category": {
      "id": "cat-1",
      "name": "Food & Dining",
      "color": "#FF6B6B",
      "iconUrl": "🍽️"
    },
    "creditCard": {
      "id": "card-456",
      "lastFourDigits": "1234",
      "cardType": "Visa"
    },
    "transactionDate": "2024-01-14T18:30:00Z",
    "status": "Completed",
    "transactionType": "Purchase",
    "rewardPoints": 45,
    "createdAt": "2024-01-14T18:30:15Z"
  },
  "message": "Transaction retrieved successfully"
}
```

#### POST /api/transactions
Create a new transaction (manual entry).

**Request:**
```json
{
  "cardId": "card-456",
  "amount": 85.50,
  "description": "Grocery shopping",
  "categoryId": "cat-3",
  "merchantName": "Whole Foods",
  "transactionDate": "2024-01-15T14:30:00Z",
  "transactionType": "Purchase"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "txn-789",
    "amount": 85.50,
    "description": "Grocery shopping",
    "merchantName": "Whole Foods",
    "category": {
      "id": "cat-3",
      "name": "Shopping",
      "color": "#45B7D1",
      "iconUrl": "🛍️"
    },
    "transactionDate": "2024-01-15T14:30:00Z",
    "status": "Completed",
    "transactionType": "Purchase",
    "rewardPoints": 86,
    "createdAt": "2024-01-15T14:30:30Z"
  },
  "message": "Transaction created successfully"
}
```

#### PUT /api/transactions/{id}
Update an existing transaction.

**Request:**
```json
{
  "description": "Updated description",
  "categoryId": "cat-2",
  "merchantName": "Updated Merchant"
}
```

#### DELETE /api/transactions/{id}
Delete a transaction (soft delete).

**Response (204 No Content)**

### 4. Credit Card Endpoints

#### GET /api/creditcards
Get user's credit cards.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "card-456",
      "cardHolderName": "John Doe",
      "lastFourDigits": "1234",
      "cardType": "Visa",
      "expiryDate": "2027-12-31T00:00:00Z",
      "creditLimit": 10000.00,
      "currentBalance": 2500.75,
      "availableBalance": 7499.25,
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "message": "Credit cards retrieved successfully"
}
```

#### POST /api/creditcards
Add a new credit card.

**Request:**
```json
{
  "cardHolderName": "John Doe",
  "lastFourDigits": "5678",
  "cardType": "Mastercard",
  "expiryDate": "2028-06-30T00:00:00Z",
  "creditLimit": 15000.00
}
```

### 5. Categories Endpoints

#### GET /api/categories
Get all transaction categories.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-1",
      "name": "Food & Dining",
      "description": "Restaurants, cafes, and food delivery",
      "iconUrl": "🍽️",
      "color": "#FF6B6B",
      "isActive": true,
      "transactionCount": 18,
      "totalSpent": 456.78
    }
  ],
  "message": "Categories retrieved successfully"
}
```

### 6. Rewards Endpoints

#### GET /api/rewards
Get user's rewards and points.

**Query Parameters:**
- `page` (number): Page number (default: 1)
- `pageSize` (number): Items per page (default: 20)
- `rewardType` (string): Filter by type ("Purchase", "Bonus", "Referral")
- `startDate` (string): Filter by earned date
- `endDate` (string): Filter by earned date

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "summary": {
      "totalPointsEarned": 15420,
      "totalPointsRedeemed": 2873,
      "availablePoints": 12547,
      "cashValue": 125.47,
      "lifetimeValue": 154.20
    },
    "rewards": [
      {
        "id": "reward-123",
        "pointsEarned": 45,
        "rewardType": "Purchase",
        "description": "Points earned from Starbucks purchase",
        "earnedDate": "2024-01-14T18:30:00Z",
        "transaction": {
          "id": "txn-123",
          "amount": 45.30,
          "merchantName": "Starbucks"
        }
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalCount": 89,
      "totalPages": 5
    }
  },
  "message": "Rewards retrieved successfully"
}
```

#### POST /api/rewards/redeem
Redeem reward points.

**Request:**
```json
{
  "pointsToRedeem": 1000,
  "redemptionType": "CashBack",
  "description": "Cash back redemption"
}
```

### 7. User Profile Endpoints

#### GET /api/users/profile
Get user profile information.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "user-123",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "preferences": {
      "currency": "USD",
      "timezone": "America/New_York",
      "notifications": {
        "email": true,
        "sms": false,
        "push": true
      }
    },
    "createdAt": "2024-01-01T00:00:00Z",
    "lastLoginAt": "2024-01-15T14:30:00Z"
  },
  "message": "Profile retrieved successfully"
}
```

#### PUT /api/users/profile
Update user profile.

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "preferences": {
    "currency": "USD",
    "timezone": "America/New_York",
    "notifications": {
      "email": true,
      "sms": false,
      "push": true
    }
  }
}
```

## Error Handling

### Standard Error Response Format

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "code": "INVALID_FORMAT",
      "message": "Email format is invalid"
    },
    {
      "field": "amount",
      "code": "OUT_OF_RANGE",
      "message": "Amount must be greater than 0"
    }
  ],
  "timestamp": "2024-01-15T14:30:00Z",
  "path": "/api/transactions"
}
```

### HTTP Status Codes

| Status Code | Description | Usage |
|-------------|-------------|--------|
| 200 | OK | Successful GET, PUT requests |
| 201 | Created | Successful POST requests |
| 204 | No Content | Successful DELETE requests |
| 400 | Bad Request | Invalid request data |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side errors |

### Error Codes

| Code | Description |
|------|-------------|
| INVALID_FORMAT | Data format is incorrect |
| REQUIRED_FIELD | Required field is missing |
| OUT_OF_RANGE | Value is outside acceptable range |
| DUPLICATE_RESOURCE | Resource already exists |
| RESOURCE_NOT_FOUND | Requested resource not found |
| UNAUTHORIZED_ACCESS | User lacks permission |
| INVALID_CREDENTIALS | Login credentials are incorrect |
| TOKEN_EXPIRED | JWT token has expired |
| RATE_LIMIT_EXCEEDED | Too many requests |

## Rate Limiting

### Rate Limit Rules

| Endpoint Pattern | Limit | Window |
|------------------|--------|---------|
| /api/auth/login | 5 requests | 15 minutes |
| /api/auth/register | 3 requests | 60 minutes |
| /api/** | 100 requests | 15 minutes |
| /api/transactions | 200 requests | 15 minutes |

### Rate Limit Headers

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640998800
Retry-After: 900
```

## API Versioning

### Version Header
```http
API-Version: 1.0
Accept: application/json;version=1.0
```

### URL Versioning (Alternative)
```
/api/v1/transactions
/api/v2/transactions
```

## Pagination

### Request Parameters
```http
GET /api/transactions?page=2&pageSize=50
```

### Response Format
```json
{
  "pagination": {
    "currentPage": 2,
    "pageSize": 50,
    "totalCount": 500,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": true,
    "nextPageUrl": "/api/transactions?page=3&pageSize=50",
    "previousPageUrl": "/api/transactions?page=1&pageSize=50"
  }
}
```

## OpenAPI/Swagger Documentation

### Swagger UI
- **Development**: https://localhost:7001/swagger
- **Production**: https://api.finpay.example.com/swagger

### OpenAPI Specification
- **JSON**: /swagger/v1/swagger.json
- **YAML**: /swagger/v1/swagger.yaml

This API design provides a comprehensive, secure, and scalable foundation for the FinPay Dashboard application, following industry best practices and standards.
# Contributing to FinPay Dashboard

Thank you for your interest in contributing to FinPay Dashboard! This guide will help you get started with the development process and understand our project structure.

## 📋 Table of Contents

- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Pull Request Process](#pull-request-process)
- [Architecture Overview](#architecture-overview)
- [API Documentation](#api-documentation)
- [Troubleshooting](#troubleshooting)

## 🚀 Getting Started

### Prerequisites

Ensure you have the following installed:

- **Node.js 18+** - [Download here](https://nodejs.org/)
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
- **Git** - [Download here](https://git-scm.com/)
- **Azure CLI** (optional) - [Installation guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

### Development Tools (Recommended)

- **Visual Studio Code** with extensions:
  - C# Dev Kit
  - ES7+ React/Redux/React-Native snippets
  - Prettier - Code formatter
  - ESLint
  - Tailwind CSS IntelliSense
  - Thunder Client (for API testing)

## 🛠️ Development Setup

### 1. Fork and Clone

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/your-username/FinPay-Dashboard.git
cd FinPay-Dashboard

# Add the original repository as upstream
git remote add upstream https://github.com/original-owner/FinPay-Dashboard.git
```

### 2. Frontend Setup

```bash
cd frontend
npm install

# Copy environment template and configure
cp .env.example .env.local
```

Configure your `.env.local`:
```env
NEXT_PUBLIC_API_URL=https://localhost:7001
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your-azure-ad-client-id
NEXT_PUBLIC_AZURE_AD_TENANT_ID=your-azure-ad-tenant-id
NEXTAUTH_SECRET=your-nextauth-secret
NEXTAUTH_URL=http://localhost:3000
```

```bash
# Start the development server
npm run dev
```

The frontend will be available at `http://localhost:3000`

### 3. Backend Setup

```bash
cd backend
dotnet restore

# Set up user secrets for development
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=FinPayDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
dotnet user-secrets set "AzureAdB2C:ClientId" "your-azure-ad-client-id"
dotnet user-secrets set "AzureAdB2C:ClientSecret" "your-azure-ad-client-secret"
dotnet user-secrets set "JwtSettings:SecretKey" "your-jwt-secret-key-minimum-32-characters"
```

### 4. Database Setup

#### Option A: Using Docker (Recommended for development)

```bash
# Start SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
   -p 1433:1433 --name sqlserver-finpay \
   -d mcr.microsoft.com/mssql/server:2022-latest

# Apply database migrations
cd backend
dotnet ef database update

# Seed with sample data (optional)
dotnet run --seed-data
```

#### Option B: Using Azure SQL Database

1. Create an Azure SQL Database
2. Update the connection string in user secrets
3. Run migrations: `dotnet ef database update`

### 5. Start the Backend

```bash
cd backend
dotnet run
```

The API will be available at `https://localhost:7001`
Swagger documentation: `https://localhost:7001/swagger`

### 6. Full Stack with Docker Compose

For a complete development environment:

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

## 📁 Project Structure

```
FinPay-Dashboard/
├── frontend/                     # Next.js React Application
│   ├── src/
│   │   ├── components/          # Reusable UI components
│   │   │   ├── ui/             # Base UI components (Shadcn)
│   │   │   ├── forms/          # Form components
│   │   │   ├── layout/         # Layout components
│   │   │   └── charts/         # Data visualization components
│   │   ├── app/                # Next.js App Router pages
│   │   ├── hooks/              # Custom React hooks
│   │   ├── lib/                # Utility libraries and configurations
│   │   ├── store/              # Redux store configuration
│   │   ├── types/              # TypeScript type definitions
│   │   └── utils/              # Helper functions
│   ├── public/                 # Static assets
│   ├── package.json
│   └── tailwind.config.js
├── backend/                     # ASP.NET Core Web API
│   ├── Controllers/            # API controllers
│   ├── Models/                 # Data models and DTOs
│   │   ├── Entities/          # Database entities
│   │   ├── DTOs/              # Data Transfer Objects
│   │   └── ViewModels/        # View models
│   ├── Services/               # Business logic services
│   │   ├── Interfaces/        # Service interfaces
│   │   └── Implementations/   # Service implementations
│   ├── Data/                   # Database context and configurations
│   │   ├── Migrations/        # EF Core migrations
│   │   └── Configurations/    # Entity configurations
│   ├── Middleware/             # Custom middleware
│   ├── Extensions/             # Extension methods
│   ├── Program.cs              # Application entry point
│   └── appsettings.json
├── tests/                      # Test projects
│   ├── Frontend.Tests/         # Frontend unit tests
│   ├── Backend.Tests/          # Backend unit tests
│   └── Integration.Tests/      # Integration tests
├── docs/                       # Documentation
├── docker-compose.yml          # Development environment
├── .github/                    # GitHub workflows and templates
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
└── README.md
```

## 🔄 Development Workflow

### Branching Strategy

We use Git Flow with the following branches:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - Feature development branches
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Critical production fixes

### Creating a Feature Branch

```bash
# Sync with upstream
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/your-feature-name

# Make your changes and commit
git add .
git commit -m "feat: add new feature description"

# Push to your fork
git push origin feature/your-feature-name
```

### Commit Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Types:
- `feat`: New features
- `fix`: Bug fixes
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```bash
feat(auth): add OAuth2 authentication
fix(api): resolve transaction filtering bug
docs(readme): update installation instructions
test(frontend): add dashboard component tests
```

## 🎨 Coding Standards

### Frontend (React/TypeScript)

#### File Naming
- Components: `PascalCase.tsx` (e.g., `TransactionList.tsx`)
- Hooks: `camelCase.ts` (e.g., `useTransactions.ts`)
- Utilities: `camelCase.ts` (e.g., `formatCurrency.ts`)
- Types: `PascalCase.types.ts` (e.g., `Transaction.types.ts`)

#### Component Structure
```typescript
import React from 'react';
import { cn } from '@/lib/utils';

interface ComponentProps {
  className?: string;
  // other props
}

export const Component: React.FC<ComponentProps> = ({
  className,
  ...props
}) => {
  return (
    <div className={cn("default-styles", className)}>
      {/* component content */}
    </div>
  );
};

export default Component;
```

#### Styling Guidelines
- Use Tailwind CSS utility classes
- Follow mobile-first responsive design
- Use CSS variables for theming
- Prefer Shadcn UI components for consistency

### Backend (C#/.NET)

#### File Naming
- Controllers: `PascalCaseController.cs`
- Services: `PascalCaseService.cs`
- Models: `PascalCase.cs`
- Interfaces: `IPascalCase.cs`

#### API Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions(
        [FromQuery] TransactionFilterDto filter)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsAsync(filter);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

#### Code Style
- Follow Microsoft C# coding conventions
- Use dependency injection
- Implement proper error handling and logging
- Use async/await for I/O operations
- Write XML documentation for public APIs

## 🧪 Testing

### Frontend Testing

```bash
cd frontend

# Run unit tests
npm test

# Run tests in watch mode
npm test:watch

# Run tests with coverage
npm test:coverage

# Run E2E tests
npm run test:e2e
```

#### Test Structure
```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { TransactionList } from './TransactionList';

describe('TransactionList', () => {
  it('renders transaction items', () => {
    const mockTransactions = [
      { id: '1', amount: 100, description: 'Test transaction' }
    ];

    render(<TransactionList transactions={mockTransactions} />);

    expect(screen.getByText('Test transaction')).toBeInTheDocument();
  });
});
```

### Backend Testing

```bash
cd backend

# Run unit tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Backend.Tests/
```

#### Test Structure
```csharp
[TestClass]
public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _service = new TransactionService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task GetTransactionsAsync_ReturnsTransactions_WhenValidFilter()
    {
        // Arrange
        var filter = new TransactionFilterDto { UserId = "test-user" };
        var expectedTransactions = new List<Transaction> { /* test data */ };
        _mockRepository.Setup(r => r.GetByFilterAsync(filter))
                      .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _service.GetTransactionsAsync(filter);

        // Assert
        Assert.AreEqual(expectedTransactions.Count, result.Count());
    }
}
```

## 📝 Pull Request Process

### Before Submitting

1. **Update your branch**:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout your-feature-branch
   git rebase develop
   ```

2. **Run tests**:
   ```bash
   # Frontend
   cd frontend && npm test && npm run lint

   # Backend
   cd backend && dotnet test && dotnet format --verify-no-changes
   ```

3. **Build successfully**:
   ```bash
   # Frontend
   cd frontend && npm run build

   # Backend
   cd backend && dotnet build --configuration Release
   ```

### PR Template

When creating a pull request, use this template:

```markdown
## Description
Brief description of the changes made.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Screenshots (if applicable)
Add screenshots to help explain your changes.

## Checklist
- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
```

### Review Process

1. **Automated Checks**: All CI checks must pass
2. **Code Review**: At least one maintainer approval required
3. **Testing**: Manual testing if UI changes are involved
4. **Documentation**: Update docs if API changes are made

## 🏗️ Architecture Overview

### System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend API   │    │    Database     │
│   (Next.js)     │◄──►│  (ASP.NET Core) │◄──►│  (Azure SQL)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Azure Static  │    │  Azure App      │    │   Azure Key     │
│   Web Apps      │    │   Service       │    │    Vault        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Frontend Architecture

- **App Router**: Next.js 14 App Router for routing
- **State Management**: Redux Toolkit for global state
- **Data Fetching**: React Query for server state
- **UI Components**: Shadcn UI + Tailwind CSS
- **Authentication**: NextAuth.js with Azure AD B2C

### Backend Architecture

- **Clean Architecture**: Separation of concerns with layers
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Built-in .NET DI container
- **Authentication**: JWT tokens with Azure AD B2C
- **Logging**: Structured logging with Serilog

### Database Design

#### Key Entities

```sql
-- Users
Users (Id, Email, FirstName, LastName, CreatedAt, UpdatedAt)

-- Credit Cards
CreditCards (Id, UserId, CardNumber, ExpiryDate, CardType, CreditLimit, AvailableBalance)

-- Transactions
Transactions (Id, CardId, Amount, Description, Category, MerchantName, TransactionDate, Status)

-- Rewards
Rewards (Id, UserId, PointsEarned, PointsRedeemed, TransactionId, EarnedDate)

-- Categories
Categories (Id, Name, Description, IconUrl)
```

## 📚 API Documentation

### Authentication

All API endpoints (except public ones) require JWT authentication:

```http
Authorization: Bearer <jwt-token>
```

### Core Endpoints

#### Transactions

```http
GET /api/transactions
Query Parameters:
- startDate: ISO date string
- endDate: ISO date string
- category: string
- merchantName: string
- pageNumber: number (default: 1)
- pageSize: number (default: 20)

Response:
{
  "data": [
    {
      "id": "string",
      "amount": number,
      "description": "string",
      "category": "string",
      "merchantName": "string",
      "transactionDate": "ISO date string",
      "status": "string"
    }
  ],
  "totalCount": number,
  "pageNumber": number,
  "pageSize": number
}
```

#### Dashboard

```http
GET /api/dashboard/summary

Response:
{
  "currentBalance": number,
  "availableCredit": number,
  "monthlySpent": number,
  "upcomingBillDate": "ISO date string",
  "upcomingBillAmount": number,
  "rewardPoints": number
}
```

For complete API documentation, run the backend and visit `/swagger`.

## 🐛 Troubleshooting

### Common Issues

#### Frontend

**Issue**: `npm install` fails with permission errors
```bash
# Solution: Use npm with correct permissions
sudo npm install -g npm@latest
npm config set registry https://registry.npmjs.org/
```

**Issue**: Environment variables not loading
```bash
# Solution: Ensure .env.local is in the frontend directory
cp frontend/.env.example frontend/.env.local
# Edit .env.local with your values
```

#### Backend

**Issue**: Database connection fails
```bash
# Solution: Check connection string and SQL Server
dotnet user-secrets list
# Verify SQL Server is running
docker ps | grep sqlserver
```

**Issue**: Migration fails
```bash
# Solution: Reset database and re-run migrations
dotnet ef database drop
dotnet ef database update
```

#### Docker

**Issue**: Container fails to start
```bash
# Solution: Check logs and rebuild
docker-compose logs
docker-compose down
docker-compose build --no-cache
docker-compose up
```

### Getting Help

1. **Check existing issues**: [GitHub Issues](https://github.com/your-username/FinPay-Dashboard/issues)
2. **Create a new issue**: Use the issue templates
3. **Join discussions**: [GitHub Discussions](https://github.com/your-username/FinPay-Dashboard/discussions)

### Development Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Azure Documentation](https://docs.microsoft.com/en-us/azure/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Shadcn UI Documentation](https://ui.shadcn.com/)

## 🎯 What to Contribute

### Good First Issues
- UI component improvements
- Additional chart types
- Frontend testing
- Documentation updates
- Bug fixes

### Advanced Contributions
- New API endpoints
- Security enhancements
- Performance optimizations
- Azure deployment configurations
- AI/ML features

### Areas Needing Help
- [ ] Mobile responsiveness improvements
- [ ] Accessibility enhancements
- [ ] Internationalization (i18n)
- [ ] Advanced filtering features
- [ ] Data export functionality
- [ ] Integration tests
- [ ] Performance monitoring

---

Thank you for contributing to FinPay Dashboard! Your efforts help make this project better for everyone. 🚀
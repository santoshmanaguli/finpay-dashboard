# Development Setup Guide

## Overview

This guide provides comprehensive instructions for setting up the FinPay Dashboard development environment. Follow these steps to get the project running locally on your machine.

## Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| [Node.js](https://nodejs.org/) | 18.x or later | Frontend runtime |
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 or later | Backend development |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Latest | Database and containers |
| [Git](https://git-scm.com/) | Latest | Version control |
| [Visual Studio Code](https://code.visualstudio.com/) | Latest | Code editor (recommended) |

### Recommended VS Code Extensions

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-typescript-next",
    "formulahendry.auto-rename-tag",
    "christian-kohler.path-intellisense",
    "ms-vscode.thunder-client",
    "humao.rest-client"
  ]
}
```

## Project Setup

### 1. Clone the Repository

```bash
# Clone the repository
git clone https://github.com/your-username/FinPay-Dashboard.git
cd FinPay-Dashboard

# Create development branch
git checkout -b feature/setup-development
```

### 2. Environment Setup

#### Backend Environment (.NET)

```bash
# Navigate to backend directory
cd backend/FinPayDashboard.Api

# Restore NuGet packages
dotnet restore

# Install Entity Framework tools globally (if not already installed)
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

#### Frontend Environment (Node.js)

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Verify installation
npm --version
node --version
```

## Database Setup

### Option 1: Docker SQL Server (Recommended)

```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 \
  --name sqlserver-finpay \
  --restart unless-stopped \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Verify container is running
docker ps | grep sqlserver-finpay

# Check container logs
docker logs sqlserver-finpay
```

### Option 2: Azure SQL Database (Free Tier)

```bash
# Login to Azure CLI
az login

# Create resource group
az group create --name rg-finpay-dev --location eastus

# Create SQL Server
az sql server create \
  --name finpay-dev-server \
  --resource-group rg-finpay-dev \
  --location eastus \
  --admin-user devadmin \
  --admin-password "YourPassword123!"

# Create database (free tier)
az sql db create \
  --resource-group rg-finpay-dev \
  --server finpay-dev-server \
  --name FinPayDevDB \
  --edition GeneralPurpose \
  --family Gen5 \
  --capacity 1 \
  --compute-model Serverless

# Allow local IP
az sql server firewall-rule create \
  --resource-group rg-finpay-dev \
  --server finpay-dev-server \
  --name AllowLocalIP \
  --start-ip-address $(curl -s https://ipinfo.io/ip) \
  --end-ip-address $(curl -s https://ipinfo.io/ip)
```

## Configuration

### Backend Configuration

#### User Secrets Setup

```bash
# Navigate to backend project
cd backend/FinPayDashboard.Api

# Initialize user secrets
dotnet user-secrets init

# Set connection string (Docker)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=FinPayDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"

# Set connection string (Azure SQL)
# dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:finpay-dev-server.database.windows.net,1433;Database=FinPayDevDB;User ID=devadmin;Password=YourPassword123!;Encrypt=True;"

# Set JWT settings
dotnet user-secrets set "JwtSettings:SecretKey" "your-development-jwt-secret-key-minimum-32-characters-long"
dotnet user-secrets set "JwtSettings:Issuer" "FinPayDashboard.Api.Dev"
dotnet user-secrets set "JwtSettings:Audience" "FinPayDashboard.Client.Dev"
dotnet user-secrets set "JwtSettings:ExpirationMinutes" "120"

# Verify secrets
dotnet user-secrets list
```

#### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Issuer": "FinPayDashboard.Api.Dev",
    "Audience": "FinPayDashboard.Client.Dev",
    "ExpirationMinutes": 120
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://localhost:3000"
    ]
  }
}
```

### Frontend Configuration

#### Environment Variables

Create `frontend/.env.local`:

```env
# API Configuration
NEXT_PUBLIC_API_URL=https://localhost:7001
NEXT_PUBLIC_API_TIMEOUT=10000

# Application Configuration
NEXT_PUBLIC_APP_NAME=FinPay Dashboard
NEXT_PUBLIC_APP_VERSION=1.0.0
NEXT_PUBLIC_APP_ENV=development

# Feature Flags
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_NOTIFICATIONS=true
NEXT_PUBLIC_ENABLE_DARK_MODE=true

# Development Settings
NEXT_PUBLIC_DEBUG_MODE=true
NEXT_PUBLIC_MOCK_API=false
```

#### Next.js Configuration

Update `frontend/next.config.js`:

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  images: {
    domains: ['localhost'],
  },
  env: {
    CUSTOM_KEY: process.env.CUSTOM_KEY,
  },
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'https://localhost:7001/api/:path*',
      },
    ]
  },
  // Development-specific settings
  experimental: {
    turbopack: true, // Enable Turbopack for faster builds
  },
}

module.exports = nextConfig
```

## Database Migration and Seeding

### Apply Migrations

```bash
# Navigate to backend project
cd backend/FinPayDashboard.Api

# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Verify database creation
dotnet ef database update --verbose
```

### Seed Development Data

Create `backend/FinPayDashboard.Api/Data/DevSeeder.cs`:

```csharp
using FinPayDashboard.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinPayDashboard.Api.Data
{
    public static class DevSeeder
    {
        public static async Task SeedAsync(FinPayDbContext context, IServiceProvider serviceProvider)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed categories
            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAsync(context);
            }

            // Seed development user
            if (!await context.Users.AnyAsync())
            {
                await SeedDevelopmentUserAsync(context);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(FinPayDbContext context)
        {
            var categories = new[]
            {
                new Category { Id = "cat-1", Name = "Food & Dining", Description = "Restaurants, cafes, and food delivery", IconUrl = "🍽️", Color = "#FF6B6B" },
                new Category { Id = "cat-2", Name = "Transportation", Description = "Gas, public transport, rideshares", IconUrl = "🚗", Color = "#4ECDC4" },
                new Category { Id = "cat-3", Name = "Shopping", Description = "Retail, online shopping, clothing", IconUrl = "🛍️", Color = "#45B7D1" },
                new Category { Id = "cat-4", Name = "Entertainment", Description = "Movies, games, subscriptions", IconUrl = "🎬", Color = "#96CEB4" },
                new Category { Id = "cat-5", Name = "Bills & Utilities", Description = "Electricity, water, internet, phone", IconUrl = "📄", Color = "#FFEAA7" },
                new Category { Id = "cat-6", Name = "Healthcare", Description = "Medical, dental, pharmacy", IconUrl = "⚕️", Color = "#FD79A8" },
                new Category { Id = "cat-7", Name = "Travel", Description = "Hotels, flights, vacation expenses", IconUrl = "✈️", Color = "#FDCB6E" },
                new Category { Id = "cat-8", Name = "Education", Description = "Courses, books, training", IconUrl = "📚", Color = "#6C5CE7" },
                new Category { Id = "cat-9", Name = "Fitness", Description = "Gym, sports, health activities", IconUrl = "💪", Color = "#A29BFE" },
                new Category { Id = "cat-10", Name = "Other", Description = "Miscellaneous expenses", IconUrl = "📦", Color = "#636E72" }
            };

            context.Categories.AddRange(categories);
        }

        private static async Task SeedDevelopmentUserAsync(FinPayDbContext context)
        {
            var devUser = new User
            {
                Id = "dev-user-123",
                Email = "dev@finpay.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("DevPassword123!"),
                FirstName = "Dev",
                LastName = "User",
                PhoneNumber = "+1234567890",
                IsActive = true
            };

            context.Users.Add(devUser);

            // Add development credit card
            var devCard = new CreditCard
            {
                Id = "dev-card-123",
                UserId = devUser.Id,
                CardNumberLastFour = "1234",
                CardHolderName = "Dev User",
                ExpiryDate = DateTime.UtcNow.AddYears(3),
                CardType = "Visa",
                CreditLimit = 10000.00m,
                AvailableBalance = 7500.00m,
                CurrentBalance = 2500.00m
            };

            context.CreditCards.Add(devCard);

            // Add sample transactions
            var transactions = GenerateSampleTransactions(devCard.Id);
            context.Transactions.AddRange(transactions);
        }

        private static List<Transaction> GenerateSampleTransactions(string cardId)
        {
            var random = new Random();
            var transactions = new List<Transaction>();
            var categories = new[] { "cat-1", "cat-2", "cat-3", "cat-4", "cat-5" };
            var merchants = new[] { "Starbucks", "Shell", "Amazon", "Netflix", "Uber" };

            for (int i = 0; i < 50; i++)
            {
                transactions.Add(new Transaction
                {
                    Id = $"dev-txn-{i + 1:D3}",
                    CardId = cardId,
                    Amount = (decimal)(random.NextDouble() * 200 + 5),
                    Description = $"Sample transaction {i + 1}",
                    CategoryId = categories[random.Next(categories.Length)],
                    MerchantName = merchants[random.Next(merchants.Length)],
                    TransactionDate = DateTime.UtcNow.AddDays(-random.Next(90)),
                    Status = "Completed",
                    TransactionType = "Purchase"
                });
            }

            return transactions;
        }
    }
}
```

Update `Program.cs` to call seeder in development:

```csharp
// In Program.cs, after app.Build()
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<FinPayDbContext>();
        await DevSeeder.SeedAsync(context, app.Services);
    }
}
```

## Running the Application

### 1. Start the Backend

```bash
# Navigate to backend project
cd backend/FinPayDashboard.Api

# Run the application
dotnet run

# Or with hot reload (recommended for development)
dotnet watch run

# The API will be available at:
# HTTP: http://localhost:5001
# HTTPS: https://localhost:7001
# Swagger: https://localhost:7001/swagger
```

### 2. Start the Frontend

```bash
# Open new terminal and navigate to frontend
cd frontend

# Start development server
npm run dev

# Or with Turbopack (faster)
npm run dev -- --turbo

# The frontend will be available at:
# http://localhost:3000
```

### 3. Verify Setup

#### Backend Health Check

```bash
# Test API health
curl -X GET https://localhost:7001/health

# Test authentication endpoint
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "dev@finpay.local",
    "password": "DevPassword123!"
  }'
```

#### Frontend Accessibility

1. Open browser to `http://localhost:3000`
2. You should see the FinPay Dashboard landing page
3. Try logging in with development credentials:
   - Email: `dev@finpay.local`
   - Password: `DevPassword123!`

## Development Workflow

### 1. Git Workflow

```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "feat: add new feature"

# Push to your fork
git push origin feature/your-feature-name

# Create pull request on GitHub
```

### 2. Backend Development

#### Adding New Entities

```bash
# Create new migration after model changes
dotnet ef migrations add AddNewEntity

# Apply migration
dotnet ef database update

# Remove last migration if needed
dotnet ef migrations remove
```

#### Running Tests

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "TestClassName"
```

### 3. Frontend Development

#### Component Development

```bash
# Install additional packages
npm install package-name

# Add Shadcn UI components
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card
npx shadcn-ui@latest add dialog

# Run type checking
npm run type-check

# Run linting
npm run lint

# Fix linting issues
npm run lint:fix
```

#### Building and Testing

```bash
# Build for production (test)
npm run build

# Start production server locally
npm start

# Run tests (when implemented)
npm test
```

## Docker Development (Alternative)

### Docker Compose Setup

Create `docker-compose.dev.yml`:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: YourPassword123!
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123! -Q 'SELECT 1'"]
      interval: 30s
      timeout: 10s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  backend:
    build:
      context: ./backend/FinPayDashboard.Api
      dockerfile: Dockerfile.dev
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Password=password
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=FinPayDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    ports:
      - "7001:443"
      - "5001:80"
    volumes:
      - ~/.aspnet/https:/https:ro
      - ./backend/FinPayDashboard.Api:/app/src
    depends_on:
      sqlserver:
        condition: service_healthy
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    environment:
      - NEXT_PUBLIC_API_URL=https://backend:443
    ports:
      - "3000:3000"
    volumes:
      - ./frontend:/app
      - /app/node_modules
    depends_on:
      - backend
    restart: unless-stopped

volumes:
  sqlserver_data:
  redis_data:
```

### Development Dockerfiles

Create `backend/FinPayDashboard.Api/Dockerfile.dev`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install development certificates
RUN dotnet dev-certs https

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet build -c Debug

ENTRYPOINT ["dotnet", "watch", "run", "--urls", "https://+:443;http://+:80"]
```

Create `frontend/Dockerfile.dev`:

```dockerfile
FROM node:18-alpine
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 3000
CMD ["npm", "run", "dev"]
```

### Run with Docker

```bash
# Start all services
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop services
docker-compose -f docker-compose.dev.yml down

# Rebuild and start
docker-compose -f docker-compose.dev.yml up --build -d
```

## Troubleshooting

### Common Issues

#### Backend Issues

| Issue | Solution |
|-------|----------|
| Database connection fails | Check Docker container status, verify connection string |
| Migration errors | Reset database: `dotnet ef database drop && dotnet ef database update` |
| Port already in use | Change port in `launchSettings.json` or kill process: `lsof -ti:7001 \| xargs kill -9` |
| SSL certificate errors | Trust development certificate: `dotnet dev-certs https --trust` |

#### Frontend Issues

| Issue | Solution |
|-------|----------|
| `npm install` fails | Clear cache: `npm cache clean --force` then retry |
| Module not found | Delete `node_modules` and `package-lock.json`, then `npm install` |
| API calls fail | Check CORS settings in backend, verify API URL in `.env.local` |
| Build fails | Check TypeScript errors: `npm run type-check` |

#### Docker Issues

| Issue | Solution |
|-------|----------|
| Container won't start | Check logs: `docker logs container-name` |
| Permission denied | Ensure Docker daemon is running and user has permissions |
| Port conflicts | Change port mappings in `docker-compose.yml` |
| Volume mount issues | Check file permissions and paths |

### Debugging

#### Backend Debugging

```bash
# Enable detailed logging
export ASPNETCORE_ENVIRONMENT=Development
export Logging__LogLevel__Default=Debug

# Debug Entity Framework queries
export Logging__LogLevel__Microsoft.EntityFrameworkCore=Information

# Run with debugger attached (VS Code)
# F5 to start debugging
```

#### Frontend Debugging

```javascript
// Enable React debugging
localStorage.setItem('debug', 'true');

// Enable Next.js debugging
DEBUG=* npm run dev

// Network debugging in browser console
// Check Network tab in Developer Tools
```

### Performance Optimization

#### Backend Performance

```csharp
// Enable response compression
builder.Services.AddResponseCompression();

// Configure Entity Framework
builder.Services.AddDbContext<FinPayDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.CommandTimeout(30);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

#### Frontend Performance

```javascript
// Enable production optimizations in development
const nextConfig = {
  // Enable source maps for debugging
  productionBrowserSourceMaps: true,

  // Optimize images
  images: {
    formats: ['image/webp', 'image/avif'],
  },

  // Enable experimental features
  experimental: {
    turbopack: true,
    optimizeCss: true,
  },
}
```

This development setup guide provides everything needed to get started with FinPay Dashboard development efficiently and effectively.
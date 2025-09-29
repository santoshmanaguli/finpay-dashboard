# Deployment Guide

## Overview

This guide covers deploying the FinPay Dashboard to production using Azure's free tier services. The deployment architecture leverages cost-effective cloud services while maintaining security and performance standards.

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Production Architecture                        │
├─────────────────────────────────────────────────────────────────┤
│  🌐 Frontend (Vercel)                                           │
│  ├── Global CDN Distribution                                    │
│  ├── Automatic SSL Certificates                                │
│  ├── Git-based Deployments                                     │
│  └── Environment Variables Management                           │
│  │                                                             │
│  ⚡ Backend (Azure Container Apps)                              │
│  ├── Auto-scaling (0 to N instances)                          │
│  ├── Container-based Deployment                                │
│  ├── Custom Domain Support                                     │
│  └── Integrated Logging                                        │
│  │                                                             │
│  💾 Database (Azure SQL Database)                               │
│  ├── 32GB Storage (Free Tier)                                 │
│  ├── Automated Backups                                         │
│  ├── High Availability                                         │
│  └── Security Features                                         │
│  │                                                             │
│  🔐 Security & Monitoring                                       │
│  ├── Azure AD B2C (Authentication)                            │
│  ├── Azure Key Vault (Secrets)                                │
│  ├── Application Insights (Monitoring)                        │
│  └── Azure Security Center                                     │
└─────────────────────────────────────────────────────────────────┘
```

## Prerequisites

### Required Accounts
- [Azure Account](https://azure.microsoft.com/free/) with free credits
- [Vercel Account](https://vercel.com/) (free tier)
- [GitHub Account](https://github.com/) for source code management
- Domain name (optional, for custom domains)

### Required Tools
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- [Node.js 18+](https://nodejs.org/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Azure Infrastructure Setup

### 1. Create Azure Resources

#### Resource Group
```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name rg-finpay-prod \
  --location eastus
```

#### Azure SQL Database (Free Tier)
```bash
# Create SQL Server
az sql server create \
  --name finpay-sql-server \
  --resource-group rg-finpay-prod \
  --location eastus \
  --admin-user finpayadmin \
  --admin-password "YourSecurePassword123!"

# Create SQL Database (Free Tier)
az sql db create \
  --resource-group rg-finpay-prod \
  --server finpay-sql-server \
  --name FinPayDB \
  --edition GeneralPurpose \
  --family Gen5 \
  --capacity 1 \
  --compute-model Serverless \
  --auto-pause-delay 60

# Configure firewall (allow Azure services)
az sql server firewall-rule create \
  --resource-group rg-finpay-prod \
  --server finpay-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### Azure Key Vault
```bash
# Create Key Vault
az keyvault create \
  --name finpay-keyvault-prod \
  --resource-group rg-finpay-prod \
  --location eastus \
  --sku standard

# Add secrets
az keyvault secret set \
  --vault-name finpay-keyvault-prod \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Server=tcp:finpay-sql-server.database.windows.net,1433;Database=FinPayDB;User ID=finpayadmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;"

az keyvault secret set \
  --vault-name finpay-keyvault-prod \
  --name "JwtSettings--SecretKey" \
  --value "your-super-secure-jwt-key-for-production-use-minimum-32-characters"
```

#### Container Registry
```bash
# Create Azure Container Registry
az acr create \
  --resource-group rg-finpay-prod \
  --name finpayregistry \
  --sku Basic \
  --admin-enabled true
```

#### Container Apps Environment
```bash
# Create Container Apps Environment
az containerapp env create \
  --name finpay-env \
  --resource-group rg-finpay-prod \
  --location eastus
```

### 2. Azure AD B2C Setup (Optional)

#### Create B2C Tenant
```bash
# Create Azure AD B2C tenant
az ad b2c tenant create \
  --resource-group rg-finpay-prod \
  --tenant-name finpayb2c \
  --country-code US \
  --display-name "FinPay B2C"
```

## Backend Deployment

### 1. Prepare Application for Production

#### Update appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Issuer": "FinPayDashboard.Api",
    "Audience": "FinPayDashboard.Client",
    "ExpirationMinutes": 60
  },
  "Azure": {
    "KeyVault": {
      "VaultUrl": "https://finpay-keyvault-prod.vault.azure.net/"
    },
    "ApplicationInsights": {
      "ConnectionString": ""
    }
  }
}
```

#### Update Program.cs for Production
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["Azure:KeyVault:VaultUrl"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<FinPayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ??
                    throw new InvalidOperationException("JWT Secret Key is required")))
        };
    });

// Add CORS for production
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production",
        policy =>
        {
            policy.WithOrigins(
                "https://finpay-dashboard.vercel.app",
                "https://www.finpay.example.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Production");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinPayDbContext>();
    context.Database.Migrate();
}

app.Run();
```

### 2. Create Dockerfile

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinPayDashboard.Api.csproj", "."]
RUN dotnet restore "./FinPayDashboard.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FinPayDashboard.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinPayDashboard.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "FinPayDashboard.Api.dll"]
```

### 3. Build and Push Container

```bash
# Build and tag the image
docker build -t finpayregistry.azurecr.io/finpay-api:latest ./backend/FinPayDashboard.Api

# Login to Azure Container Registry
az acr login --name finpayregistry

# Push the image
docker push finpayregistry.azurecr.io/finpay-api:latest
```

### 4. Deploy to Container Apps

```bash
# Create Container App
az containerapp create \
  --name finpay-api \
  --resource-group rg-finpay-prod \
  --environment finpay-env \
  --image finpayregistry.azurecr.io/finpay-api:latest \
  --target-port 80 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 3 \
  --cpu 0.25 \
  --memory 0.5Gi \
  --registry-server finpayregistry.azurecr.io \
  --registry-username finpayregistry \
  --registry-password $(az acr credential show --name finpayregistry --query passwords[0].value -o tsv) \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    AZURE_CLIENT_ID=secretref:azure-client-id \
  --secrets \
    azure-client-id=$(az account show --query id -o tsv)
```

## Frontend Deployment

### 1. Prepare Frontend for Production

#### Update Environment Variables
Create `.env.production`:
```env
NEXT_PUBLIC_API_URL=https://finpay-api.politeglacier-12345678.eastus.azurecontainerapps.io
NEXT_PUBLIC_APP_NAME=FinPay Dashboard
NEXT_PUBLIC_APP_VERSION=1.0.0
```

#### Update next.config.js
```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['localhost'],
    unoptimized: true
  },
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
  },
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'DENY',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'Referrer-Policy',
            value: 'origin-when-cross-origin',
          },
        ],
      },
    ]
  },
}

module.exports = nextConfig
```

### 2. Deploy to Vercel

#### Connect GitHub Repository
1. Login to [Vercel Dashboard](https://vercel.com/dashboard)
2. Click "New Project"
3. Import your GitHub repository
4. Select the `frontend` directory as the root

#### Configure Build Settings
```json
{
  "buildCommand": "npm run build",
  "outputDirectory": ".next",
  "installCommand": "npm install",
  "framework": "nextjs"
}
```

#### Set Environment Variables
In Vercel dashboard, add:
- `NEXT_PUBLIC_API_URL`
- `NEXTAUTH_SECRET`
- `NEXTAUTH_URL`

#### Custom Domain (Optional)
1. Add custom domain in Vercel settings
2. Configure DNS records:
   ```
   Type: CNAME
   Name: www
   Value: cname.vercel-dns.com

   Type: A
   Name: @
   Value: 76.76.19.19
   ```

## Database Migration

### 1. Run Migrations in Production

```bash
# Update connection string for production
export ConnectionStrings__DefaultConnection="Server=tcp:finpay-sql-server.database.windows.net,1433;Database=FinPayDB;User ID=finpayadmin;Password=YourSecurePassword123!;Encrypt=True;"

# Run migrations
dotnet ef database update --project backend/FinPayDashboard.Api
```

### 2. Seed Production Data

```csharp
// Create a seeding script
public static class ProductionSeeder
{
    public static async Task SeedAsync(FinPayDbContext context)
    {
        // Seed categories if they don't exist
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Id = "cat-1", Name = "Food & Dining", Description = "Restaurants, cafes, and food delivery", IconUrl = "🍽️", Color = "#FF6B6B" },
                new Category { Id = "cat-2", Name = "Transportation", Description = "Gas, public transport, rideshares", IconUrl = "🚗", Color = "#4ECDC4" },
                // ... other categories
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
```

## CI/CD Pipeline

### 1. GitHub Actions Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  AZURE_CONTAINER_REGISTRY: finpayregistry.azurecr.io
  CONTAINER_APP_NAME: finpay-api
  RESOURCE_GROUP: rg-finpay-prod

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore backend/FinPayDashboard.Api

    - name: Build
      run: dotnet build backend/FinPayDashboard.Api --no-restore

    - name: Test
      run: dotnet test backend/FinPayDashboard.Api --no-build --verbosity normal

  build-and-deploy-backend:
    if: github.ref == 'refs/heads/main'
    needs: test
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3

    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Build and push container image
      uses: azure/container-apps-deploy-action@v1
      with:
        appSourcePath: ${{ github.workspace }}/backend/FinPayDashboard.Api
        registryUrl: ${{ env.AZURE_CONTAINER_REGISTRY }}
        registryUsername: ${{ secrets.REGISTRY_USERNAME }}
        registryPassword: ${{ secrets.REGISTRY_PASSWORD }}
        containerAppName: ${{ env.CONTAINER_APP_NAME }}
        resourceGroup: ${{ env.RESOURCE_GROUP }}
        imageToBuild: ${{ env.AZURE_CONTAINER_REGISTRY }}/finpay-api:${{ github.sha }}

  deploy-frontend:
    if: github.ref == 'refs/heads/main'
    needs: test
    runs-on: ubuntu-latest
    steps:
    - name: Deploy to Vercel
      uses: amondnet/vercel-action@v20
      with:
        vercel-token: ${{ secrets.VERCEL_TOKEN }}
        vercel-org-id: ${{ secrets.ORG_ID }}
        vercel-project-id: ${{ secrets.PROJECT_ID }}
        working-directory: ./frontend
```

### 2. Required Secrets

Add these secrets to your GitHub repository:

| Secret Name | Description |
|-------------|-------------|
| `AZURE_CREDENTIALS` | Azure service principal credentials |
| `REGISTRY_USERNAME` | Azure Container Registry username |
| `REGISTRY_PASSWORD` | Azure Container Registry password |
| `VERCEL_TOKEN` | Vercel deployment token |
| `ORG_ID` | Vercel organization ID |
| `PROJECT_ID` | Vercel project ID |

## Monitoring and Logging

### 1. Application Insights Setup

```bash
# Create Application Insights
az monitor app-insights component create \
  --app finpay-insights \
  --location eastus \
  --resource-group rg-finpay-prod \
  --application-type web
```

### 2. Configure Logging

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Add structured logging
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
        .WriteTo.Console());
```

### 3. Health Checks

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContext<FinPayDbContext>()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

// Configure health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Security Configuration

### 1. Network Security

```bash
# Configure network security group
az network nsg create \
  --resource-group rg-finpay-prod \
  --name finpay-nsg

# Allow HTTPS traffic
az network nsg rule create \
  --resource-group rg-finpay-prod \
  --nsg-name finpay-nsg \
  --name allow-https \
  --protocol tcp \
  --priority 1000 \
  --destination-port-range 443 \
  --access allow
```

### 2. SSL/TLS Configuration

```csharp
// In Program.cs for production
if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

### 3. Security Headers

```csharp
// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");

    await next();
});
```

## Backup and Disaster Recovery

### 1. Database Backup Strategy

```bash
# Configure automated backups
az sql db update \
  --resource-group rg-finpay-prod \
  --server finpay-sql-server \
  --name FinPayDB \
  --backup-storage-redundancy Geo
```

### 2. Application Backup

```bash
# Export container app configuration
az containerapp show \
  --name finpay-api \
  --resource-group rg-finpay-prod \
  --output json > finpay-api-config.json
```

## Scaling and Performance

### 1. Auto-scaling Configuration

```bash
# Update container app scaling rules
az containerapp update \
  --name finpay-api \
  --resource-group rg-finpay-prod \
  --min-replicas 0 \
  --max-replicas 10 \
  --scale-rule-name http-requests \
  --scale-rule-type http \
  --scale-rule-metadata concurrentRequests=100
```

### 2. Performance Monitoring

```csharp
// Add performance counters
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableEventCounterCollectionModule = true;
});
```

## Cost Optimization

### 1. Monitor Azure Costs

```bash
# Set up cost alerts
az consumption budget create \
  --resource-group rg-finpay-prod \
  --budget-name finpay-monthly-budget \
  --amount 50 \
  --time-grain Monthly \
  --time-period start-date=2024-01-01 end-date=2024-12-31
```

### 2. Resource Optimization

- **Container Apps**: Use scale-to-zero for development environments
- **SQL Database**: Use serverless tier with auto-pause
- **Storage**: Use cool tier for backups and logs
- **Monitoring**: Use basic tier of Application Insights

This deployment guide provides a comprehensive approach to deploying FinPay Dashboard to production while maintaining security, performance, and cost-effectiveness.
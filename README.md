1. Business Requirement Document (BRD)

Purpose

Create a web-based credit card management dashboard that mirrors and extends the features of the existing OneCard mobile app. The objective is to demonstrate full-stack skills (React + .NET + Azure), build a fintech-grade portfolio project, and showcase ability to design scalable, secure systems.

Goals

Provide a responsive web application for managing credit card accounts.

Implement a secure backend API using C# + ASP.NET Core, deployed on Azure.

Integrate realistic financial features:

Card management

Transaction history

Spending insights

Reward tracking

Deliver a production-ready architecture (CI/CD, monitoring, secure secrets).

Out of Scope

Real banking integrations (mock data will be used).

PCI-DSS compliance (not required for demo).

Payment processing with real networks.

2. What Are We Developing

Web App Name: FinPay Dashboard (placeholder – rename later).

Core Features:

Authentication

Login/Signup (OAuth2, Azure AD B2C, or IdentityServer).

Multi-factor mock (OTP/email for demo).

Dashboard

Current balance

Available limit

Upcoming bill due date

Monthly spend summary (charts)

Transactions

Transaction list with filters (date, category, merchant)

Transaction details view

Export as CSV

Rewards

Track earned rewards

Redeem mock rewards

Gamification elements (badges, milestones)

Profile & Settings

User profile management

Notifications settings

Linked bank accounts (mock)

AI Insights (Bonus)

Categorize spending (food, travel, shopping, etc.)

Predict overspending trends

Recommend saving opportunities

3. Tech Stack
Frontend

React + Next.js (TypeScript)

Styling: Tailwind CSS + Shadcn UI

State Management: Redux Toolkit (for global state) + React Query (for API data)

Charts: Recharts / Chart.js

Auth: OAuth2/OpenID Connect (via Azure AD B2C)

Backend

C# + ASP.NET Core Web API

Database: Azure SQL Database

ORM: Entity Framework Core

Auth: Azure AD B2C / IdentityServer

Deployment: Azure App Service

Monitoring: Azure Application Insights

Secrets: Azure Key Vault

DevOps

Docker (containerize frontend + backend)

CI/CD → GitHub Actions → Azure deployment

Branching strategy → main (production), dev (staging)

4. System Architecture

[Frontend: React/Next.js] <---> [Backend API: ASP.NET Core] <---> [Azure SQL DB]
        |                               |                          |
   Hosted on Vercel/Azure Static   Hosted on Azure App Service   Managed on Azure
        |
   Auth handled via Azure AD B2C

5. Project Roadmap
Phase 1 – Week 1–2: Setup & Core

Setup GitHub repo (monorepo or separate repos for FE/BE).

Scaffold Next.js frontend + ASP.NET Core backend.

Setup Azure SQL DB & ORM.

Implement auth flow (login/signup with JWT from IdentityServer/Azure AD B2C).

Basic UI: landing + login.

Phase 2 – Week 3–4: Dashboard & Transactions

Build Dashboard page with dummy data.

Implement Transactions API (GET, filter).

Frontend → Transaction list with search/filter.

Add charts for monthly spend.

Phase 3 – Week 5–6: Rewards & Profile

Backend: Rewards API (mock).

Frontend: Rewards UI + redemption flow.

Profile & Settings page.

Phase 4 – Week 7: AI Insights (Optional but cool)

Build simple categorization logic (merchant name → category).

Add predictive overspending alert (rule-based).

Display insights on dashboard.

Phase 5 – Week 8: Production Hardening

Add CI/CD pipeline → GitHub Actions → Azure.

Add Application Insights logging.

Deploy demo version online.

Write README + Developer Docs.

6. Deliverables

Web app deployed on Azure/Vercel.

Source code repo with clear commits & branches.

Documentation:

README (how to run locally, APIs, deployment).

API docs (Swagger for backend).

User guide (screenshots + features).

Demo video (walkthrough for portfolio).

7. Future Enhancements (If you want to stretch)

PWA (so the web app can act like a mobile app too).

Notifications (Azure Notification Hub).

Real-time updates (SignalR/WebSockets).

Bank API integration (via sandbox, like RazorpayX or Plaid equivalent in India).


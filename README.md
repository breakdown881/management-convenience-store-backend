# Convenience Store Chain Management System (Backend API)
> **Enterprise Retail ERP & Web POS Backend Solution**  
> Built with **.NET 8/9**, **PostgreSQL**, **Redis**, and structured around **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS**.

[![.NET](https://img.shields.io/badge/.NET-8.0%20%2F%209.0-purple.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-blue.svg)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7+-red.svg)](https://redis.io/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20CQRS-brightgreen.svg)]()
[![Status](https://img.shields.io/badge/Status-In%20Development-yellow.svg)]()
[![License](https://img.shields.io/badge/License-MIT-blue.svg)]()

---

## 📖 1. Project Overview

The **Convenience Store Chain Management System** is an enterprise-grade backend system designed to power retail convenience store chains (modeled after industry leaders such as **7-Eleven, Circle K, GS25, FamilyMart, and WinMart+**).

The solution unifies two mission-critical subsystems:
1. **Store-Level Web POS API:** Built for cashiers and store staff at checkout counters. Features sub-**$100\text{ms}$** barcode scanning responses, **Offline-First** transaction synchronization, strict cash register shift management (**Blind Drop Reconciliation**), POS hardware integration (ESC/POS thermal printers, automated cash drawer kickout, customer display screens), and digital fiscal receipt generation (**e-Invoice**).
2. **Headquarters Back-Office / ERP API:** Centralized command center for store managers, warehouse supervisors, procurement teams, finance, and chain executives. Features cross-store replenishment, central Distribution Center (DC) coordination, **3-Way Matching** vendor procurement, **FEFO (First Expired, First Out)** lot tracking, flexible **Promotion Engine**, Quick Commerce integration (**GrabMart, ShopeeFood**), and automated **Store P&L (EBITDA)** analytics.

---

## 🏛️ 2. System Architecture

The project strictly adheres to **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)** principles, ensuring high maintainability, testability, and technology decoupling.

```
[ Clients: Web POS / Back-Office Admin / Mobile App / Food Partner Webhooks ]
                                       │ (HTTPS RESTful / SignalR)
                                       ▼
                             [ API Gateway / YARP ]
                                       │ (Rate Limiting, Reverse Proxy, Idempotency)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           ASP.NET CORE WEB API                              │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ Presentation Layer: Minimal APIs / Controllers / SignalR Hubs         │  │
│  └───────────────────────────────────┬───────────────────────────────────┘  │
│                                      ▼                                      │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ Application Layer: MediatR CQRS, FluentValidation, Pipeline Behaviors │  │
│  │ - Commands: EF Core (CUD operations, Unit of Work, Business Rules)    │  │
│  │ - Queries: Dapper (High-performance Read-only Barcode/Reports)        │  │
│  └───────────────────────────────────┬───────────────────────────────────┘  │
│                                      ▼                                      │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ Domain Layer: Pure C# Domain Model (No external dependencies)         │  │
│  │ - Entities, Value Objects, Domain Events, Enums, Domain Exceptions    │  │
│  └───────────────────────────────────┬───────────────────────────────────┘  │
│                                      ▼                                      │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ Infrastructure Layer:                                                 │  │
│  │ - Persistence: PostgreSQL (Npgsql + EF Core), Dapper                  │  │
│  │ - Caching & Locking: Redis (Barcode & Pricebook Cache, RedLock)       │  │
│  │ - Messaging: Outbox Worker, RabbitMQ / Apache Kafka                   │  │
│  │ - Third-party: Dynamic VietQR, MoMo, VNPay, e-Invoice Integrations    │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
         ┌─────────────────────────────┼─────────────────────────────┐
         ▼                             ▼                             ▼
[ PostgreSQL 16+ ]               [ Redis 7+ ]             [ RabbitMQ / Kafka ]
(Primary DB, ACID, JSONB)     (Cache Barcode/Pricebook,   (Async Background Jobs,
                               Distributed Locks)          Loyalty, e-Invoice Sync)
```

### Key Technical Highlights:
- **CQS Separation:** **Entity Framework Core** handles transactional state changes, aggregates, and business invariant validations. **Dapper** executes high-performance read-only queries directly against database indexes for sub-5ms lookups.
- **High Concurrency & Inventory Protection:** Combines Optimistic Concurrency Control (PostgreSQL `xmin` / row versions) and Distributed Locks (via Redis RedLock) to eliminate race conditions when multiple POS registers sell the same inventory item during peak rush hours.
- **Transactional Outbox Pattern:** Guarantees atomic domain event publishing (e.g., deducting BOM recipe ingredients, awarding loyalty points, triggering e-invoices) by persisting outbox messages within the same database transaction, polled concurrently using PostgreSQL's `SELECT ... FOR UPDATE SKIP LOCKED`.
- **API Idempotency:** Enforces the `X-Idempotency-Key` header on checkout, payment, stock adjustments, and goods receipts to prevent duplicate executions from network retries.

---

## 🗂️ 3. Project Directory Structure

```text
manage-convenience-store/
├── src/
│   ├── ConvenienceStore.Domain/           # Enterprise Business Rules, Entities & Events
│   │   ├── Common/                        # BaseEntity, IAggregateRoot, ValueObject
│   │   ├── Enums/                         # OrderStatus, ShiftStatus, PaymentMethod...
│   │   ├── Entities/                      # Product, Store, Inventory, Order, Shift...
│   │   └── Events/                        # Domain Events (e.g., OrderCompletedDomainEvent)
│   │
│   ├── ConvenienceStore.Application/      # Use Cases & Application Orchestration
│   │   ├── Common/                        # Behaviors (Validation, Logging, Caching)
│   │   ├── Features/                      # Vertical CQRS Slices (Commands & Queries)
│   │   │   ├── Products/
│   │   │   ├── Orders/
│   │   │   ├── Shifts/
│   │   │   └── Inventories/
│   │   └── Interfaces/                    # Abstractions (IAppDbContext, ICurrentUserService)
│   │
│   ├── ConvenienceStore.Infrastructure/   # External Services & Technical Implementations
│   │   ├── Persistence/                   # AppDbContext, Entity Configurations, Migrations
│   │   ├── Repositories/                  # Dapper Query Repositories & EF Repositories
│   │   ├── Cache/                         # Redis Cache Service & Distributed Locks
│   │   ├── Messaging/                     # Outbox Background Processor, RabbitMQ/Kafka
│   │   └── Payments/                      # VietQR, MoMo, VNPay Providers
│   │
│   └── ConvenienceStore.Api/              # Entry point Web API
│       ├── Controllers/                   # RESTful Controllers / Minimal APIs
│       ├── Middlewares/                   # Global Exception Handling, Idempotency
│       └── Extensions/                    # Dependency Injection Bootstrappers
│
├── tests/
│   ├── ConvenienceStore.Domain.Tests/     # Domain Invariant & Unit Tests
│   ├── ConvenienceStore.Application.Tests/# Handler & Business Logic Unit Tests
│   └── ConvenienceStore.IntegrationTests/ # Integration Tests with Testcontainers
│
├── docker/
│   └── docker-compose.yml                 # Local Infrastructure (PostgreSQL, Redis, pgAdmin)
│
├── FEATURES.md                            # Comprehensive Functional & Technical Specs
├── ROADMAP.md                             # 56-Task Milestone Tracker & Progress Checklist
└── README.md                              # Project Documentation
```

---

## 💻 4. Technology Stack & Dependencies

| Layer / Role | Technology | Description |
| :--- | :--- | :--- |
| **Runtime & Framework** | **.NET 8.0 / .NET 9.0 SDK** | C# 12 / 13 features |
| **Primary Database** | **PostgreSQL 16+** | Relational ACID, JSONB indexing, `SKIP LOCKED` |
| **Caching & Concurrency** | **Redis 7+ (StackExchange.Redis)** | Pricebook/Barcode caching, Distributed Lock (RedLock) |
| **ORM & Micro-ORM** | **EF Core 8/9 + Dapper** | Transactional Unit of Work + Ultra-fast Queries |
| **CQRS & Mediator** | **MediatR** | In-process messaging and pipeline behaviors |
| **Validation** | **FluentValidation** | Strongly-typed request validation |
| **Authentication & RBAC** | **ASP.NET Core Identity + JWT** | 6 Enterprise system roles (ACT-01 to ACT-06) |
| **Structured Logging** | **Serilog + OpenTelemetry** | Distributed tracing and structured JSON logs |
| **Message Broker** | **RabbitMQ / Apache Kafka** | Event-driven background tasks and async pipelines |
| **Containerization** | **Docker & Docker Compose** | Reproducible development & production environments |

---

## 🚀 5. Getting Started (Local Development)

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (supporting Docker Compose)
- Any IDE: JetBrains Rider, Visual Studio 2022, or VS Code with C# Dev Kit.

### Step 1: Start Supporting Infrastructure
Spin up the local PostgreSQL, Redis, and pgAdmin containers:
```bash
docker compose -f docker/docker-compose.yml up -d
```

### Step 2: Configure Connection Strings
Verify connection strings in `src/ConvenienceStore.Api/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ConvenienceStoreDb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  }
}
```

### Step 3: Apply Database Migrations
Run Entity Framework Core migrations to provision database schemas:
```bash
dotnet ef database update --project src/ConvenienceStore.Infrastructure --startup-project src/ConvenienceStore.Api
```

### Step 4: Run the Backend API
```bash
dotnet run --project src/ConvenienceStore.Api
```
- Interactive Swagger / OpenAPI Documentation is accessible at:  
  `https://localhost:5001/swagger` or `http://localhost:5000/swagger`.

---

## 📋 6. API Conventions & Standards

1. **RESTful Standard Envelope Pattern:**
   ```json
   {
     "success": true,
     "data": { ... },
     "message": "Operation completed successfully",
     "errors": null,
     "timestamp": "2026-10-13T11:35:00Z"
   }
   ```
2. **RFC 7807 ProblemDetails Error Format:** All unexpected exceptions and business validation failures are formatted via `GlobalExceptionHandlerMiddleware` with standard HTTP error codes.
3. **Idempotency Header:** All critical mutations (checkout, payment, inventory adjustments) require:
   ```http
   X-Idempotency-Key: 9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d
   ```
4. **Automated Audit Trail:** System entities inheriting from `AuditableEntity` automatically track `CreatedAtUtc`, `CreatedBy`, `LastModifiedAtUtc`, and `LastModifiedBy` through EF Core save interceptors.

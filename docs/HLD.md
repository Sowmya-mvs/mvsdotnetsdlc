# High Level Design (HLD) — Banking Application

## 1. Overview

This document describes the High Level Design for a **Banking Application** built with ASP.NET Core (.NET 8). The application follows **Clean Architecture** principles and exposes a RESTful Web API consumed by front-end or third-party clients.

---

## 2. Goals and Scope

| Goal | Description |
|---|---|
| Customer Management | Create and manage bank customers (personal details, KYC). |
| Account Management | Open, close, and manage bank accounts (Savings, Checking). |
| Transactions | Deposit, withdrawal, and fund transfer between accounts. |
| Authentication | JWT-based auth; role-based access (Admin, Teller, Customer). |
| Audit & History | Full transaction history per account. |

---

## 3. Architecture Style

The solution uses **Clean Architecture** (Onion / Ports & Adapters) with four concentric layers:

```
┌──────────────────────────────────────────────────┐
│                    API Layer                     │  ← ASP.NET Core Web API
│   Controllers · Middleware · Request/Response    │
├──────────────────────────────────────────────────┤
│               Application Layer                  │  ← Use-cases, DTOs, Interfaces
│         Services · Commands · Validators         │
├──────────────────────────────────────────────────┤
│                 Domain Layer                     │  ← Business Rules (no dependencies)
│       Entities · Aggregates · Domain Events      │
├──────────────────────────────────────────────────┤
│             Infrastructure Layer                 │  ← EF Core, Repositories, External
│          Database · Email · Logging              │
└──────────────────────────────────────────────────┘
```

---

## 4. System Context Diagram

```
          ┌─────────────┐
          │  Web/Mobile │
          │   Client    │
          └──────┬──────┘
                 │ HTTPS / REST
          ┌──────▼──────────────────┐
          │   Banking API           │
          │  (ASP.NET Core 8)       │
          └──┬──────────┬──────────┘
             │          │
    ┌─────────▼──┐  ┌───▼──────────┐
    │ SQL        │  │  JWT Auth    │
    │ Database   │  │  Service     │
    │ (SQLite /  │  └──────────────┘
    │  SQL Srv)  │
    └────────────┘
```

---

## 5. Key Components

### 5.1 Modules

| Module | Responsibilities |
|---|---|
| **Customer** | Register, update, retrieve customer profiles. |
| **Account** | Open account, close account, get balance, account details. |
| **Transaction** | Deposit, withdraw, transfer, transaction history. |
| **Authentication** | Login, JWT token generation, role management. |

### 5.2 Technology Stack

| Component | Technology |
|---|---|
| Backend Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database (default) | SQLite (easily swappable to SQL Server / PostgreSQL) |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation |
| Logging | Microsoft.Extensions.Logging (Serilog-ready) |
| Testing | xUnit + Moq |
| API Docs | Swagger / OpenAPI (Swashbuckle) |

### 5.3 Security

- **JWT Bearer tokens** — short-lived access tokens (60 min) issued on login.
- **Role-based Authorization** — `Admin`, `Teller`, `Customer` roles.
- **HTTPS** enforced in production.
- **Password hashing** via ASP.NET Core Identity (PBKDF2).
- **Input validation** on all endpoints.
- No sensitive data stored in plain text.

---

## 6. Data Flow

### Deposit Flow

```
Client → POST /api/transactions/deposit
       → TransactionsController
       → TransactionService.DepositAsync()
       → AccountRepository (load account)
       → Account.Deposit() [domain logic]
       → TransactionRepository.AddAsync()
       → SaveChangesAsync()
       → Return TransactionDto (200 OK)
```

### Transfer Flow

```
Client → POST /api/transactions/transfer
       → TransactionsController
       → TransactionService.TransferAsync()
       → AccountRepository (load source + target)
       → Account.Debit() / Account.Credit() [domain logic]
       → TransactionRepository.AddAsync() × 2
       → SaveChangesAsync()
       → Return TransferResultDto (200 OK)
```

---

## 7. API Endpoints (Summary)

| Method | Endpoint | Description |
|---|---|---|
| POST | /api/auth/register | Register a new user |
| POST | /api/auth/login | Authenticate and obtain JWT |
| GET | /api/customers/{id} | Get customer by ID |
| POST | /api/customers | Create customer |
| PUT | /api/customers/{id} | Update customer |
| GET | /api/accounts/{id} | Get account details & balance |
| POST | /api/accounts | Open a new account |
| DELETE | /api/accounts/{id} | Close an account |
| GET | /api/accounts/{id}/transactions | Transaction history |
| POST | /api/transactions/deposit | Deposit funds |
| POST | /api/transactions/withdraw | Withdraw funds |
| POST | /api/transactions/transfer | Transfer between accounts |

---

## 8. Non-Functional Requirements

| Requirement | Target |
|---|---|
| Availability | 99.9% uptime |
| Performance | < 300 ms p95 API response time |
| Scalability | Stateless API — horizontally scalable |
| Security | OWASP Top 10 compliance |
| Observability | Structured logging, health checks |

---

## 9. Deployment View

```
┌─────────────────────────────────┐
│  Container / App Service        │
│  ┌───────────────────────────┐  │
│  │  BankingApp.API (Docker)  │  │
│  └───────────────────────────┘  │
│  ┌───────────────────────────┐  │
│  │  SQLite / SQL Server      │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
```

---

*Document version: 1.0 | Date: 2026-03-13*

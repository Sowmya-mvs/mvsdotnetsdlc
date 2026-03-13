# mvsdotnetsdlc — Banking Application

A fully functional **Banking Application** built with **ASP.NET Core 8** following Clean Architecture principles.

## Documentation

| Document | Description |
|---|---|
| [HLD.md](docs/HLD.md) | High Level Design — architecture, components, data flow, API summary |
| [LLD.md](docs/LLD.md) | Low Level Design — class design, DB schema, sequence diagrams |

## Solution Structure

```
BankingApp/
├── src/
│   ├── BankingApp.Domain/          # Entities, enums, repository interfaces
│   ├── BankingApp.Application/     # Application services, DTOs, service interfaces
│   ├── BankingApp.Infrastructure/  # EF Core DbContext, repository implementations
│   └── BankingApp.API/             # ASP.NET Core Web API (controllers, middleware)
└── tests/
    └── BankingApp.Tests/           # xUnit unit tests
```

## Features

- **Customer Management** — Create, update, and retrieve customer profiles
- **Account Management** — Open Savings/Checking accounts, get balance, close accounts
- **Transactions** — Deposit, withdrawal, and fund transfer between accounts
- **Transaction History** — Full per-account transaction history
- **JWT Authentication** — Register/login endpoints returning Bearer tokens
- **Role-Based Authorization** — Admin, Teller, Customer roles
- **Swagger UI** — Interactive API documentation at `/swagger`
- **Global Error Handling** — RFC 7807 ProblemDetails responses
- **Health Checks** — `/health` endpoint

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

```bash
# Restore and build
dotnet build BankingApp.slnx

# Run the API
dotnet run --project src/BankingApp.API

# Run tests
dotnet test BankingApp.slnx
```

Once running, open **https://localhost:5001/swagger** for the interactive API docs.

## API Quick Reference

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | None | Register a user |
| POST | /api/auth/login | None | Authenticate (returns JWT) |
| POST | /api/customers | Teller/Admin | Create customer |
| GET | /api/customers/{id} | Any | Get customer |
| PUT | /api/customers/{id} | Teller/Admin | Update customer |
| POST | /api/accounts | Teller/Admin | Open account |
| GET | /api/accounts/{id} | Any | Get account |
| DELETE | /api/accounts/{id} | Admin | Close account |
| GET | /api/accounts/{id}/transactions | Any | Transaction history |
| POST | /api/transactions/deposit | Teller/Admin | Deposit |
| POST | /api/transactions/withdraw | Teller/Admin | Withdraw |
| POST | /api/transactions/transfer | Teller/Admin | Transfer |

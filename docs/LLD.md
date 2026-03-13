# Low Level Design (LLD) — Banking Application

## 1. Overview

This document provides the detailed low-level design for the Banking Application, expanding on the HLD with class diagrams, database schema, sequence diagrams, and design decisions.

---

## 2. Solution Structure

```
BankingApp/
├── src/
│   ├── BankingApp.Domain/          # Core business entities (no dependencies)
│   │   ├── Entities/
│   │   │   ├── Customer.cs
│   │   │   ├── Account.cs
│   │   │   └── Transaction.cs
│   │   ├── Enums/
│   │   │   ├── AccountType.cs
│   │   │   └── TransactionType.cs
│   │   └── Interfaces/
│   │       ├── IAccountRepository.cs
│   │       ├── ICustomerRepository.cs
│   │       └── ITransactionRepository.cs
│   ├── BankingApp.Application/     # Use cases and application services
│   │   ├── DTOs/
│   │   │   ├── AccountDto.cs
│   │   │   ├── CustomerDto.cs
│   │   │   └── TransactionDto.cs
│   │   ├── Interfaces/
│   │   │   ├── IAccountService.cs
│   │   │   ├── ICustomerService.cs
│   │   │   └── ITransactionService.cs
│   │   └── Services/
│   │       ├── AccountService.cs
│   │       ├── CustomerService.cs
│   │       └── TransactionService.cs
│   ├── BankingApp.Infrastructure/  # EF Core, repositories, identity
│   │   ├── Data/
│   │   │   └── BankingDbContext.cs
│   │   └── Repositories/
│   │       ├── AccountRepository.cs
│   │       ├── CustomerRepository.cs
│   │       └── TransactionRepository.cs
│   └── BankingApp.API/             # ASP.NET Core entry point
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── AccountsController.cs
│       │   ├── CustomersController.cs
│       │   └── TransactionsController.cs
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── BankingApp.Tests/           # xUnit unit tests
        ├── AccountServiceTests.cs
        ├── CustomerServiceTests.cs
        └── TransactionServiceTests.cs
```

---

## 3. Domain Layer — Class Design

### 3.1 Customer Entity

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Account> Accounts { get; }

    // Domain factory + update methods
    public static Customer Create(string firstName, string lastName, string email,
                                  string phone, DateTime dob, string address);
    public void Update(string firstName, string lastName, string phone, string address);
}
```

### 3.2 Account Entity

```csharp
public class Account
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; }   // e.g., "ACC-0000001"
    public Guid CustomerId { get; private set; }
    public AccountType Type { get; private set; }       // Savings | Checking
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public IReadOnlyCollection<Transaction> Transactions { get; }

    // Business methods (enforce invariants)
    public void Deposit(decimal amount);   // throws if amount <= 0
    public void Withdraw(decimal amount);  // throws if insufficient funds
    public void Close();                   // sets IsActive = false
}
```

### 3.3 Transaction Entity

```csharp
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }   // Deposit | Withdrawal | Transfer
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? RelatedAccountId { get; private set; } // For transfers

    public static Transaction Create(Guid accountId, TransactionType type,
                                     decimal amount, decimal balanceAfter,
                                     string description, Guid? relatedAccountId = null);
}
```

### 3.4 Enumerations

```csharp
public enum AccountType  { Savings, Checking }
public enum TransactionType { Deposit, Withdrawal, Transfer }
```

---

## 4. Application Layer — DTOs and Service Contracts

### 4.1 DTOs

```csharp
// CustomerDto
record CustomerDto(Guid Id, string FirstName, string LastName,
                   string Email, string Phone, DateTime DateOfBirth,
                   string Address, DateTime CreatedAt);

// AccountDto
record AccountDto(Guid Id, string AccountNumber, Guid CustomerId,
                  string Type, decimal Balance, bool IsActive,
                  DateTime OpenedAt, DateTime? ClosedAt);

// TransactionDto
record TransactionDto(Guid Id, Guid AccountId, string Type,
                      decimal Amount, decimal BalanceAfter,
                      string Description, DateTime CreatedAt,
                      Guid? RelatedAccountId);

// Request records (input models)
record CreateCustomerRequest(string FirstName, string LastName,
                              string Email, string Phone,
                              DateTime DateOfBirth, string Address);
record OpenAccountRequest(Guid CustomerId, string AccountType);
record DepositRequest(Guid AccountId, decimal Amount, string Description);
record WithdrawRequest(Guid AccountId, decimal Amount, string Description);
record TransferRequest(Guid SourceAccountId, Guid TargetAccountId,
                       decimal Amount, string Description);
```

### 4.2 Service Interfaces

```csharp
// ICustomerService
Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);

// IAccountService
Task<AccountDto> OpenAccountAsync(OpenAccountRequest request);
Task<AccountDto?> GetAccountByIdAsync(Guid id);
Task<IEnumerable<AccountDto>> GetAccountsByCustomerIdAsync(Guid customerId);
Task CloseAccountAsync(Guid id);

// ITransactionService
Task<TransactionDto> DepositAsync(DepositRequest request);
Task<TransactionDto> WithdrawAsync(WithdrawRequest request);
Task<(TransactionDto debit, TransactionDto credit)> TransferAsync(TransferRequest request);
Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid accountId);
```

---

## 5. Database Schema

### 5.1 ER Diagram

```
Customers
─────────
PK  Id               UNIQUEIDENTIFIER
    FirstName        NVARCHAR(100)
    LastName         NVARCHAR(100)
    Email            NVARCHAR(200) UNIQUE
    Phone            NVARCHAR(20)
    DateOfBirth      DATE
    Address          NVARCHAR(500)
    CreatedAt        DATETIME2

Accounts
────────
PK  Id               UNIQUEIDENTIFIER
FK  CustomerId       → Customers.Id
    AccountNumber    NVARCHAR(20)  UNIQUE
    Type             INT           (0=Savings, 1=Checking)
    Balance          DECIMAL(18,2)
    IsActive         BIT
    OpenedAt         DATETIME2
    ClosedAt         DATETIME2 NULL

Transactions
────────────
PK  Id               UNIQUEIDENTIFIER
FK  AccountId        → Accounts.Id
    Type             INT           (0=Deposit, 1=Withdrawal, 2=Transfer)
    Amount           DECIMAL(18,2)
    BalanceAfter     DECIMAL(18,2)
    Description      NVARCHAR(500)
    CreatedAt        DATETIME2
    RelatedAccountId UNIQUEIDENTIFIER NULL
```

### 5.2 EF Core DbContext

```csharp
public class BankingDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account>  Accounts  => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Customer
        modelBuilder.Entity<Customer>(e => {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Email).IsUnique();
        });
        // Account → Customer (one-to-many)
        modelBuilder.Entity<Account>(e => {
            e.HasKey(a => a.Id);
            e.HasOne<Customer>()
             .WithMany(c => c.Accounts)
             .HasForeignKey(a => a.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        // Transaction → Account (one-to-many)
        modelBuilder.Entity<Transaction>(e => {
            e.HasKey(t => t.Id);
            e.HasOne<Account>()
             .WithMany(a => a.Transactions)
             .HasForeignKey(t => t.AccountId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

---

## 6. API Layer — Controller Design

### 6.1 AccountsController

| Route | Method | Auth | Description |
|---|---|---|---|
| /api/accounts | POST | Teller/Admin | Open new account |
| /api/accounts/{id} | GET | Any | Get account details |
| /api/accounts/customer/{customerId} | GET | Any | Accounts for a customer |
| /api/accounts/{id} | DELETE | Admin | Close account |
| /api/accounts/{id}/transactions | GET | Any | Transaction history |

### 6.2 CustomersController

| Route | Method | Auth | Description |
|---|---|---|---|
| /api/customers | POST | Teller/Admin | Create customer |
| /api/customers/{id} | GET | Any | Get customer by ID |
| /api/customers | GET | Admin | List all customers |
| /api/customers/{id} | PUT | Teller/Admin | Update customer |

### 6.3 TransactionsController

| Route | Method | Auth | Description |
|---|---|---|---|
| /api/transactions/deposit | POST | Teller/Admin | Deposit funds |
| /api/transactions/withdraw | POST | Teller/Admin | Withdraw funds |
| /api/transactions/transfer | POST | Teller/Admin | Transfer funds |

### 6.4 AuthController

| Route | Method | Auth | Description |
|---|---|---|---|
| /api/auth/register | POST | None | Register a user |
| /api/auth/login | POST | None | Authenticate (returns JWT) |

---

## 7. Error Handling

- A global `ExceptionHandlingMiddleware` converts exceptions to RFC 7807 `ProblemDetails` responses.

| Exception Type | HTTP Status |
|---|---|
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 400 Bad Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| Unhandled exceptions | 500 Internal Server Error |

---

## 8. Authentication and Authorization Design

- On `/api/auth/login` success → issue `JwtSecurityToken` (60-minute expiry).
- Claims: `sub` (userId), `email`, `role` (Admin | Teller | Customer).
- Controllers use `[Authorize(Roles = "Admin,Teller")]` or `[Authorize]`.

---

## 9. Unit Test Design

| Test Class | Method Under Test | Scenario |
|---|---|---|
| `AccountServiceTests` | `OpenAccountAsync` | Happy path — account created |
| `AccountServiceTests` | `OpenAccountAsync` | Customer not found → exception |
| `AccountServiceTests` | `CloseAccountAsync` | Account closed successfully |
| `TransactionServiceTests` | `DepositAsync` | Amount > 0 — balance updated |
| `TransactionServiceTests` | `WithdrawAsync` | Insufficient funds → exception |
| `TransactionServiceTests` | `TransferAsync` | Successful transfer |
| `CustomerServiceTests` | `CreateCustomerAsync` | Customer created |
| `CustomerServiceTests` | `GetCustomerByIdAsync` | Not found returns null |

---

*Document version: 1.0 | Date: 2026-03-13*

namespace BankingApp.Application.DTOs;

public record AccountDto(
    Guid Id,
    string AccountNumber,
    Guid CustomerId,
    string Type,
    decimal Balance,
    bool IsActive,
    DateTime OpenedAt,
    DateTime? ClosedAt);

public record OpenAccountRequest(
    Guid CustomerId,
    string AccountType);

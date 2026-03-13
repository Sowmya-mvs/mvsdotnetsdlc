namespace BankingApp.Application.DTOs;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTime CreatedAt,
    Guid? RelatedAccountId);

public record DepositRequest(
    Guid AccountId,
    decimal Amount,
    string Description);

public record WithdrawRequest(
    Guid AccountId,
    decimal Amount,
    string Description);

public record TransferRequest(
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    string Description);

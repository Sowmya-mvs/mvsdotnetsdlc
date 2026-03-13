using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public Guid? RelatedAccountId { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        Guid accountId,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        string description,
        Guid? relatedAccountId = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId must not be empty.", nameof(accountId));
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Description = description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            RelatedAccountId = relatedAccountId
        };
    }
}

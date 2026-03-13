using BankingApp.Domain.Enums;

namespace BankingApp.Domain.Entities;

public class Account
{
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { }

    public static Account Create(Guid customerId, AccountType type, string accountNumber)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId must not be empty.", nameof(customerId));

        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);

        return new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            CustomerId = customerId,
            Type = type,
            Balance = 0m,
            IsActive = true,
            OpenedAt = DateTime.UtcNow
        };
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Deposit amount must be greater than zero.");
        if (!IsActive)
            throw new InvalidOperationException("Cannot deposit into a closed account.");

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be greater than zero.");
        if (!IsActive)
            throw new InvalidOperationException("Cannot withdraw from a closed account.");
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds.");

        Balance -= amount;
    }

    public void Close()
    {
        if (!IsActive)
            throw new InvalidOperationException("Account is already closed.");
        if (Balance != 0)
            throw new InvalidOperationException("Cannot close an account with a non-zero balance.");

        IsActive = false;
        ClosedAt = DateTime.UtcNow;
    }
}

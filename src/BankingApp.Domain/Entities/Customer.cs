namespace BankingApp.Domain.Entities;

public class Customer
{
    private readonly List<Account> _accounts = new();

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime dateOfBirth,
        string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            DateOfBirth = dateOfBirth,
            Address = address.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string firstName, string lastName, string phone, string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone.Trim();
        Address = address.Trim();
    }
}

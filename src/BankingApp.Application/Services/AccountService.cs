using BankingApp.Application.DTOs;
using BankingApp.Application.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Interfaces;

namespace BankingApp.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;

    public AccountService(IAccountRepository accountRepository, ICustomerRepository customerRepository)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
    }

    public async Task<AccountDto> OpenAccountAsync(
        OpenAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found.");

        if (!Enum.TryParse<AccountType>(request.AccountType, ignoreCase: true, out var accountType))
            throw new InvalidOperationException($"Invalid account type '{request.AccountType}'. Use 'Savings' or 'Checking'.");

        var count = await _accountRepository.CountAsync(cancellationToken);
        var accountNumber = $"ACC-{(count + 1):D7}";

        var account = Account.Create(customer.Id, accountType, accountNumber);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return ToDto(account);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        return account is null ? null : ToDto(account);
    }

    public async Task<IEnumerable<AccountDto>> GetAccountsByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return accounts.Select(ToDto);
    }

    public async Task CloseAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID '{id}' not found.");

        account.Close();
        await _accountRepository.SaveChangesAsync(cancellationToken);
    }

    private static AccountDto ToDto(Account a) =>
        new(a.Id, a.AccountNumber, a.CustomerId, a.Type.ToString(), a.Balance, a.IsActive, a.OpenedAt, a.ClosedAt);
}

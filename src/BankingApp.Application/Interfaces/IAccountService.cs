using BankingApp.Application.DTOs;

namespace BankingApp.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDto> OpenAccountAsync(OpenAccountRequest request, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountDto>> GetAccountsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task CloseAccountAsync(Guid id, CancellationToken cancellationToken = default);
}

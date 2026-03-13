using BankingApp.Application.DTOs;

namespace BankingApp.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
}

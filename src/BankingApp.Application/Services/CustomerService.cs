using BankingApp.Application.DTOs;
using BankingApp.Application.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;

namespace BankingApp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> CreateCustomerAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"A customer with email '{request.Email}' already exists.");

        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.DateOfBirth,
            request.Address);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(ToDto);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID '{id}' not found.");

        customer.Update(request.FirstName, request.LastName, request.Phone, request.Address);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    private static CustomerDto ToDto(Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.DateOfBirth, c.Address, c.CreatedAt);
}

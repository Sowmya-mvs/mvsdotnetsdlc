using BankingApp.Application.DTOs;
using BankingApp.Application.Services;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Moq;

namespace BankingApp.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateCustomerAsync_ValidRequest_ReturnsCustomerDto()
    {
        var request = new CreateCustomerRequest("Jane", "Doe", "jane@example.com", "555-1234",
            new DateTime(1990, 1, 1), "123 Main St");

        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Customer?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await _sut.CreateCustomerAsync(request);

        Assert.NotNull(result);
        Assert.Equal("jane", result.Email.Split('@')[0]);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task CreateCustomerAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var existingCustomer = Customer.Create("John", "Doe", "john@example.com", "555-0000",
            DateTime.UtcNow.AddYears(-30), "1 Test Rd");

        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingCustomer);

        var request = new CreateCustomerRequest("John", "Doe", "john@example.com", "555-0000",
            DateTime.UtcNow.AddYears(-30), "1 Test Rd");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateCustomerAsync(request));
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ExistingId_ReturnsDto()
    {
        var customer = Customer.Create("Alice", "Smith", "alice@example.com", "555-9999",
            new DateTime(1985, 6, 15), "5 Oak Lane");

        _repoMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(customer);

        var result = await _sut.GetCustomerByIdAsync(customer.Id);

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result!.Id);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Customer?)null);

        var result = await _sut.GetCustomerByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCustomerAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Customer?)null);

        var request = new UpdateCustomerRequest("Bob", "Jones", "555-0001", "New Address");

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.UpdateCustomerAsync(Guid.NewGuid(), request));
    }
}

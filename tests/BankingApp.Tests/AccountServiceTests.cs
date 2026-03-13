using BankingApp.Application.DTOs;
using BankingApp.Application.Services;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Interfaces;
using Moq;

namespace BankingApp.Tests;

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _sut = new AccountService(_accountRepoMock.Object, _customerRepoMock.Object);
    }

    [Fact]
    public async Task OpenAccountAsync_ValidRequest_ReturnsAccountDto()
    {
        var customer = Customer.Create("Bob", "Builder", "bob@example.com", "555-0002",
            new DateTime(1980, 3, 20), "99 Build St");

        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(customer);
        _accountRepoMock.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(0);
        _accountRepoMock.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);
        _accountRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        var request = new OpenAccountRequest(customer.Id, "Savings");
        var result = await _sut.OpenAccountAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Savings", result.Type);
        Assert.Equal(0m, result.Balance);
        Assert.True(result.IsActive);
        Assert.Equal("ACC-0000001", result.AccountNumber);
    }

    [Fact]
    public async Task OpenAccountAsync_CustomerNotFound_ThrowsKeyNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Customer?)null);

        var request = new OpenAccountRequest(Guid.NewGuid(), "Savings");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.OpenAccountAsync(request));
    }

    [Fact]
    public async Task OpenAccountAsync_InvalidAccountType_ThrowsInvalidOperationException()
    {
        var customer = Customer.Create("Carol", "Chen", "carol@example.com", "555-0003",
            new DateTime(1975, 9, 10), "7 Pine Ave");

        _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(customer);
        _accountRepoMock.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var request = new OpenAccountRequest(customer.Id, "Invalid");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.OpenAccountAsync(request));
    }

    [Fact]
    public async Task CloseAccountAsync_AccountNotFound_ThrowsKeyNotFoundException()
    {
        _accountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Account?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CloseAccountAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CloseAccountAsync_ZeroBalance_ClosesSuccessfully()
    {
        var account = Account.Create(Guid.NewGuid(), AccountType.Savings, "ACC-0000001");

        _accountRepoMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(account);
        _accountRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        await _sut.CloseAccountAsync(account.Id);

        Assert.False(account.IsActive);
    }
}

using BankingApp.Application.DTOs;
using BankingApp.Application.Services;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Interfaces;
using Moq;

namespace BankingApp.Tests;

public class TransactionServiceTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<ITransactionRepository> _txRepoMock = new();
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        _sut = new TransactionService(_accountRepoMock.Object, _txRepoMock.Object);
    }

    private static Account CreateActiveAccount(decimal initialBalance = 0m)
    {
        var account = Account.Create(Guid.NewGuid(), AccountType.Savings, $"ACC-{Guid.NewGuid():N}");
        if (initialBalance > 0)
            account.Deposit(initialBalance);
        return account;
    }

    [Fact]
    public async Task DepositAsync_ValidAmount_ReturnsTransactionDto()
    {
        var account = CreateActiveAccount();

        _accountRepoMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(account);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        _accountRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        var request = new DepositRequest(account.Id, 500m, "Initial deposit");
        var result = await _sut.DepositAsync(request);

        Assert.NotNull(result);
        Assert.Equal(500m, result.Amount);
        Assert.Equal(500m, result.BalanceAfter);
        Assert.Equal("Deposit", result.Type);
    }

    [Fact]
    public async Task DepositAsync_AccountNotFound_ThrowsKeyNotFoundException()
    {
        _accountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Account?)null);

        var request = new DepositRequest(Guid.NewGuid(), 100m, "test");
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DepositAsync(request));
    }

    [Fact]
    public async Task WithdrawAsync_SufficientFunds_ReturnsTransactionDto()
    {
        var account = CreateActiveAccount(initialBalance: 1000m);

        _accountRepoMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(account);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        _accountRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        var request = new WithdrawRequest(account.Id, 300m, "ATM withdrawal");
        var result = await _sut.WithdrawAsync(request);

        Assert.NotNull(result);
        Assert.Equal(300m, result.Amount);
        Assert.Equal(700m, result.BalanceAfter);
        Assert.Equal("Withdrawal", result.Type);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientFunds_ThrowsInvalidOperationException()
    {
        var account = CreateActiveAccount(initialBalance: 50m);

        _accountRepoMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(account);

        var request = new WithdrawRequest(account.Id, 200m, "Over-limit withdrawal");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.WithdrawAsync(request));
    }

    [Fact]
    public async Task TransferAsync_ValidAccounts_ReturnsBothTransactions()
    {
        var source = CreateActiveAccount(initialBalance: 500m);
        var target = CreateActiveAccount();

        _accountRepoMock.Setup(r => r.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(source);
        _accountRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(target);
        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        _accountRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        var request = new TransferRequest(source.Id, target.Id, 200m, "Transfer");
        var (debit, credit) = await _sut.TransferAsync(request);

        Assert.Equal(200m, debit.Amount);
        Assert.Equal(300m, debit.BalanceAfter);
        Assert.Equal(200m, credit.Amount);
        Assert.Equal(200m, credit.BalanceAfter);
    }

    [Fact]
    public async Task TransferAsync_SameAccount_ThrowsInvalidOperationException()
    {
        var accountId = Guid.NewGuid();
        var request = new TransferRequest(accountId, accountId, 100m, "Self-transfer");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.TransferAsync(request));
    }

    [Fact]
    public async Task TransferAsync_InsufficientFunds_ThrowsInvalidOperationException()
    {
        var source = CreateActiveAccount(initialBalance: 50m);
        var target = CreateActiveAccount();

        _accountRepoMock.Setup(r => r.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(source);
        _accountRepoMock.Setup(r => r.GetByIdAsync(target.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(target);

        var request = new TransferRequest(source.Id, target.Id, 200m, "Over-limit transfer");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.TransferAsync(request));
    }
}

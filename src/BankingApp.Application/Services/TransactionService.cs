using BankingApp.Application.DTOs;
using BankingApp.Application.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Interfaces;

namespace BankingApp.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionDto> DepositAsync(
        DepositRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID '{request.AccountId}' not found.");

        account.Deposit(request.Amount);

        var transaction = Transaction.Create(
            account.Id,
            TransactionType.Deposit,
            request.Amount,
            account.Balance,
            request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    public async Task<TransactionDto> WithdrawAsync(
        WithdrawRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account with ID '{request.AccountId}' not found.");

        account.Withdraw(request.Amount);

        var transaction = Transaction.Create(
            account.Id,
            TransactionType.Withdrawal,
            request.Amount,
            account.Balance,
            request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    public async Task<(TransactionDto Debit, TransactionDto Credit)> TransferAsync(
        TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceAccountId == request.TargetAccountId)
            throw new InvalidOperationException("Source and target accounts must be different.");

        var source = await _accountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Source account with ID '{request.SourceAccountId}' not found.");

        var target = await _accountRepository.GetByIdAsync(request.TargetAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Target account with ID '{request.TargetAccountId}' not found.");

        source.Withdraw(request.Amount);
        target.Deposit(request.Amount);

        var debit = Transaction.Create(
            source.Id,
            TransactionType.Transfer,
            request.Amount,
            source.Balance,
            request.Description,
            target.Id);

        var credit = Transaction.Create(
            target.Id,
            TransactionType.Transfer,
            request.Amount,
            target.Balance,
            request.Description,
            source.Id);

        await _transactionRepository.AddAsync(debit, cancellationToken);
        await _transactionRepository.AddAsync(credit, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        return (ToDto(debit), ToDto(credit));
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId, cancellationToken);
        return transactions.Select(ToDto);
    }

    private static TransactionDto ToDto(Transaction t) =>
        new(t.Id, t.AccountId, t.Type.ToString(), t.Amount, t.BalanceAfter, t.Description, t.CreatedAt, t.RelatedAccountId);
}

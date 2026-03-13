using BankingApp.Application.DTOs;

namespace BankingApp.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);
    Task<TransactionDto> WithdrawAsync(WithdrawRequest request, CancellationToken cancellationToken = default);
    Task<(TransactionDto Debit, TransactionDto Credit)> TransferAsync(TransferRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid accountId, CancellationToken cancellationToken = default);
}

using BankingApp.Application.DTOs;
using BankingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Teller")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>Deposit funds into an account.</summary>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.DepositAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Withdraw funds from an account.</summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.WithdrawAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Transfer funds between two accounts.</summary>
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
    {
        var (debit, credit) = await _transactionService.TransferAsync(request, cancellationToken);
        return Ok(new { Debit = debit, Credit = credit });
    }
}

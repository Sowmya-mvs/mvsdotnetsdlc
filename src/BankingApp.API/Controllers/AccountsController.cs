using BankingApp.Application.DTOs;
using BankingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;

    public AccountsController(IAccountService accountService, ITransactionService transactionService)
    {
        _accountService = accountService;
        _transactionService = transactionService;
    }

    /// <summary>Open a new bank account.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teller")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OpenAccount(
        [FromBody] OpenAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await _accountService.OpenAccountAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>Get account details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        return account is null ? NotFound() : Ok(account);
    }

    /// <summary>Get all accounts for a customer.</summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var accounts = await _accountService.GetAccountsByCustomerIdAsync(customerId, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>Close an account.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseAccount(Guid id, CancellationToken cancellationToken)
    {
        await _accountService.CloseAccountAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Get transaction history for an account.</summary>
    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _transactionService.GetTransactionHistoryAsync(id, cancellationToken);
        return Ok(history);
    }
}

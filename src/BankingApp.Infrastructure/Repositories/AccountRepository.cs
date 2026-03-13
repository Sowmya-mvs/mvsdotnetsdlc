using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default) =>
        await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AddAsync(account, cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        await _context.Accounts.CountAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}

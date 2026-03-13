using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankingDbContext _context;

    public CustomerRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Customers.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Customers.ToListAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await _context.Customers.AddAsync(customer, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}

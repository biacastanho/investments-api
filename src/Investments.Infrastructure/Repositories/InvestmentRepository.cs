using Investments.Domain.Entities;
using Investments.Domain.Repositories;
using Investments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Investments.Infrastructure.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly AppDbContext _db;
    public InvestmentRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Investment>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Investments.AsNoTracking().Include(i => i.Type)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.InvestedAt)
            .ToListAsync(ct);

    public Task<Investment?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _db.Investments.Include(i => i.Type).FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    public async Task AddAsync(Investment investment, CancellationToken ct = default)
        => await _db.Investments.AddAsync(investment, ct);

    public void Update(Investment investment) => _db.Investments.Update(investment);

    public void Remove(Investment investment) => _db.Investments.Remove(investment);
}

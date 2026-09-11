using Investments.Domain.Entities;
using Investments.Domain.Repositories;
using Investments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Investments.Infrastructure.Repositories;

public class InvestmentTypeRepository : IInvestmentTypeRepository
{
    private readonly AppDbContext _db;
    public InvestmentTypeRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvestmentType>> GetAllAsync(CancellationToken ct = default)
        => await _db.InvestmentTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    public Task<InvestmentType?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.InvestmentTypes.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<InvestmentType?> GetByNameAsync(string name, CancellationToken ct = default)
        => _db.InvestmentTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), ct);

    public Task<bool> NameExistsAsync(string name, int? exceptId = null, CancellationToken ct = default)
        => _db.InvestmentTypes.AnyAsync(t => t.Name.ToLower() == name.ToLower() && (exceptId == null || t.Id != exceptId), ct);

    public Task<bool> IsInUseAsync(int id, CancellationToken ct = default)
        => _db.Investments.AnyAsync(i => i.InvestmentTypeId == id, ct);

    public async Task AddAsync(InvestmentType type, CancellationToken ct = default)
        => await _db.InvestmentTypes.AddAsync(type, ct);

    public void Update(InvestmentType type) => _db.InvestmentTypes.Update(type);

    public void Remove(InvestmentType type) => _db.InvestmentTypes.Remove(type);
}

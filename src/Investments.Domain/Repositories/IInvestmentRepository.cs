using Investments.Domain.Entities;

namespace Investments.Domain.Repositories;

public interface IInvestmentRepository
{
    Task<IReadOnlyList<Investment>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Investment?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task AddAsync(Investment investment, CancellationToken ct = default);
    void Update(Investment investment);
    void Remove(Investment investment);
}

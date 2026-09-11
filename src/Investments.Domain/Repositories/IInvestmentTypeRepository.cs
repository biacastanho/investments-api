using Investments.Domain.Entities;

namespace Investments.Domain.Repositories;

public interface IInvestmentTypeRepository
{
    Task<IReadOnlyList<InvestmentType>> GetAllAsync(CancellationToken ct = default);
    Task<InvestmentType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<InvestmentType?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? exceptId = null, CancellationToken ct = default);

    /// <summary>Indica se o tipo já está sendo usado por algum investimento (bloqueia a exclusão).</summary>
    Task<bool> IsInUseAsync(int id, CancellationToken ct = default);

    Task AddAsync(InvestmentType type, CancellationToken ct = default);
    void Update(InvestmentType type);
    void Remove(InvestmentType type);
}

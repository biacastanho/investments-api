using Investments.Application.DTOs;
using Investments.Application.Exceptions;
using Investments.Application.Interfaces;
using Investments.Domain.Entities;
using Investments.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Investments.Application.Services;

public class InvestmentTypeService : IInvestmentTypeService
{
    private readonly IInvestmentTypeRepository _types;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InvestmentTypeService> _logger;

    public InvestmentTypeService(IInvestmentTypeRepository types, IUnitOfWork uow, ILogger<InvestmentTypeService> logger)
    {
        _types = types;
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InvestmentTypeResponse>> ListAsync(CancellationToken ct = default)
    {
        var list = await _types.GetAllAsync(ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<InvestmentTypeResponse> GetAsync(int id, CancellationToken ct = default)
        => ToResponse(await GetExistingAsync(id, ct));

    public async Task<InvestmentTypeResponse> CreateAsync(InvestmentTypeRequest request, CancellationToken ct = default)
    {
        await EnsureNameIsFreeAsync(request.Name, null, ct);

        var type = new InvestmentType(request.Name);
        await _types.AddAsync(type, ct);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("Tipo de investimento {TypeId} ({Name}) criado", type.Id, type.Name);
        return ToResponse(type);
    }

    public async Task<InvestmentTypeResponse> UpdateAsync(int id, InvestmentTypeRequest request, CancellationToken ct = default)
    {
        var type = await GetExistingAsync(id, ct);
        await EnsureNameIsFreeAsync(request.Name, id, ct);

        type.Rename(request.Name);
        _types.Update(type);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("Tipo de investimento {TypeId} renomeado para {Name}", id, type.Name);
        return ToResponse(type);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var type = await GetExistingAsync(id, ct);

        // Não deixa órfãos: um tipo em uso por investimentos não pode ser removido.
        if (await _types.IsInUseAsync(id, ct))
            throw new ConflictException($"O tipo '{type.Name}' está em uso por investimentos e não pode ser removido.");

        _types.Remove(type);
        await _uow.CommitAsync(ct);
        _logger.LogInformation("Tipo de investimento {TypeId} removido", id);
    }

    private async Task<InvestmentType> GetExistingAsync(int id, CancellationToken ct)
        => await _types.GetByIdAsync(id, ct)
           ?? throw new NotFoundException("Tipo de investimento não encontrado.");

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken ct)
    {
        if (await _types.NameExistsAsync(name.Trim(), exceptId, ct))
            throw new ConflictException($"Já existe um tipo de investimento chamado '{name.Trim()}'.");
    }

    private static InvestmentTypeResponse ToResponse(InvestmentType t) =>
        new(t.Id, t.Name, t.CreatedAt, t.UpdatedAt);
}

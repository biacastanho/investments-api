using Investments.Application.DTOs;
using Investments.Application.Exceptions;
using Investments.Application.Interfaces;
using Investments.Domain.Entities;
using Investments.Domain.Exceptions;
using Investments.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Investments.Application.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _investments;
    private readonly IInvestmentTypeRepository _types;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InvestmentService> _logger;

    public InvestmentService(
        IInvestmentRepository investments,
        IInvestmentTypeRepository types,
        IUnitOfWork uow,
        ILogger<InvestmentService> logger)
    {
        _investments = investments;
        _types = types;
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InvestmentResponse>> ListAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _investments.GetByUserAsync(userId, ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<InvestmentResponse> GetAsync(Guid userId, Guid id, CancellationToken ct = default)
        => ToResponse(await GetOwnedAsync(userId, id, ct));

    public async Task<InvestmentResponse> CreateAsync(Guid userId, InvestmentRequest request, CancellationToken ct = default)
    {
        var type = await ResolveTypeAsync(request.Type, ct);

        var investment = new Investment(userId, type.Id, request.Amount, request.InvestedAt!.Value, request.Description);
        await _investments.AddAsync(investment, ct);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("Investimento {InvestmentId} criado para usuário {UserId}", investment.Id, userId);
        return ToResponse(investment, type);
    }

    public async Task<InvestmentResponse> UpdateAsync(Guid userId, Guid id, InvestmentRequest request, CancellationToken ct = default)
    {
        var investment = await GetOwnedAsync(userId, id, ct);
        var type = await ResolveTypeAsync(request.Type, ct);

        investment.Update(type.Id, request.Amount, request.InvestedAt!.Value, request.Description);
        _investments.Update(investment);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("Investimento {InvestmentId} atualizado pelo usuário {UserId}", id, userId);
        return ToResponse(investment, type);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var investment = await GetOwnedAsync(userId, id, ct);
        _investments.Remove(investment);
        await _uow.CommitAsync(ct);
        _logger.LogInformation("Investimento {InvestmentId} removido pelo usuário {UserId}", id, userId);
    }

    // Controle de acesso: só retorna o investimento se pertencer ao usuário autenticado.
    private async Task<Investment> GetOwnedAsync(Guid userId, Guid id, CancellationToken ct)
        => await _investments.GetByIdAsync(id, userId, ct)
           ?? throw new NotFoundException("Investimento não encontrado.");

    /// <summary>Converte o nome informado no request no tipo cadastrado, com erro legível quando não existe.</summary>
    private async Task<InvestmentType> ResolveTypeAsync(string name, CancellationToken ct)
    {
        var type = await _types.GetByNameAsync(name.Trim(), ct);
        if (type is not null) return type;

        var disponiveis = string.Join(", ", (await _types.GetAllAsync(ct)).Select(t => t.Name));
        throw new DomainException(
            $"Tipo de investimento '{name}' não existe. Tipos disponíveis: {disponiveis}. " +
            "Novos tipos podem ser cadastrados em POST /investment-types.");
    }

    private static InvestmentResponse ToResponse(Investment i) =>
        ToResponse(i, i.Type);

    private static InvestmentResponse ToResponse(Investment i, InvestmentType? type) =>
        new(i.Id, i.InvestmentTypeId, type?.Name ?? string.Empty, i.Amount, i.InvestedAt,
            i.Description, i.CreatedAt, i.UpdatedAt);
}

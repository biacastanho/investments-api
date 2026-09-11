using Investments.Domain.Exceptions;

namespace Investments.Domain.Entities;

/// <summary>
/// Tipo de investimento (Acoes, RendaFixa, Fundos...). Deixou de ser um enum para
/// permitir manutenção em runtime pelos endpoints /investment-types.
/// </summary>
public class InvestmentType
{
    public const int MaxNameLength = 50;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Investment> _investments = new();
    public IReadOnlyCollection<Investment> Investments => _investments.AsReadOnly();

    private InvestmentType() { }

    public InvestmentType(string name)
    {
        Name = Normalize(name);
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string name)
    {
        Name = Normalize(name);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Normalize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de investimento é obrigatório.");

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            throw new DomainException($"Nome do tipo deve ter no máximo {MaxNameLength} caracteres.");

        return trimmed;
    }
}

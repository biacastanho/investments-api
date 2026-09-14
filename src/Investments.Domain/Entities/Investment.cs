using Investments.Domain.Exceptions;

namespace Investments.Domain.Entities;

public class Investment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int InvestmentTypeId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime InvestedAt { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public InvestmentType? Type { get; private set; }

    private Investment() { }

    public Investment(Guid userId, int investmentTypeId, decimal amount, DateTime investedAt, string? description)
    {
        if (userId == Guid.Empty) throw new DomainException("Usuário é obrigatório.");
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        SetValues(investmentTypeId, amount, investedAt, description);
    }

    public void Update(int investmentTypeId, decimal amount, DateTime investedAt, string? description)
    {
        SetValues(investmentTypeId, amount, investedAt, description);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetValues(int investmentTypeId, decimal amount, DateTime investedAt, string? description)
    {
        if (investmentTypeId <= 0) 
            throw new DomainException("Tipo de investimento é obrigatório.");
        
        if (amount <= 0) 
            throw new DomainException("Valor investido deve ser maior que zero.");

        if (investedAt == default) 
            throw new DomainException("Data de investimento é obrigatória.");

        if (investedAt > DateTime.UtcNow) 
            throw new DomainException("Data de investimento não pode ser futura.");

        if (description is { Length: > 200 }) 
            throw new DomainException("Descrição deve ter no máximo 200 caracteres.");

        InvestmentTypeId = investmentTypeId;
        Amount = amount;
        InvestedAt = investedAt;
        Description = description?.Trim();
    }
}

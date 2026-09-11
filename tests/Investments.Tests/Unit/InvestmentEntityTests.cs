using FluentAssertions;
using Investments.Domain.Entities;
using Investments.Domain.Exceptions;

namespace Investments.Tests.Unit;

public class InvestmentEntityTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private const int Acoes = 1;
    private const int RendaFixa = 2;
    private const int Fundos = 3;
    private const int Tesouro = 4;

    [Fact]
    public void Deve_criar_investimento_valido()
    {
        var investedAt = DateTime.UtcNow.AddDays(-1);
        var inv = new Investment(UserId, Acoes, 1500.50m, investedAt, "PETR4");

        inv.Id.Should().NotBeEmpty();
        inv.UserId.Should().Be(UserId);
        inv.InvestmentTypeId.Should().Be(Acoes);
        inv.Amount.Should().Be(1500.50m);
        inv.InvestedAt.Should().Be(investedAt);
        inv.Description.Should().Be("PETR4");
        inv.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Nao_deve_aceitar_valor_menor_ou_igual_a_zero(decimal amount)
    {
        var act = () => new Investment(UserId, RendaFixa, amount, DateTime.UtcNow, null);
        act.Should().Throw<DomainException>().WithMessage("*maior que zero*");
    }

    [Fact]
    public void Nao_deve_aceitar_data_futura()
    {
        var act = () => new Investment(UserId, Fundos, 100, DateTime.UtcNow.AddDays(10), null);
        act.Should().Throw<DomainException>().WithMessage("*futura*");
    }

    [Fact]
    public void Nao_deve_aceitar_data_nao_informada()
    {
        var act = () => new Investment(UserId, Fundos, 100, default, null);
        act.Should().Throw<DomainException>().WithMessage("*Data de investimento*");
    }

    [Fact]
    public void Nao_deve_aceitar_tipo_invalido()
    {
        var act = () => new Investment(UserId, 0, 100, DateTime.UtcNow, null);
        act.Should().Throw<DomainException>().WithMessage("*Tipo*");
    }

    [Fact]
    public void Nao_deve_aceitar_usuario_vazio()
    {
        var act = () => new Investment(Guid.Empty, Acoes, 100, DateTime.UtcNow, null);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_deve_alterar_valores_e_marcar_UpdatedAt()
    {
        var inv = new Investment(UserId, Acoes, 100, DateTime.UtcNow, null);

        inv.Update(Tesouro, 250, DateTime.UtcNow.AddDays(-2), "Selic");

        inv.InvestmentTypeId.Should().Be(Tesouro);
        inv.Amount.Should().Be(250);
        inv.Description.Should().Be("Selic");
        inv.UpdatedAt.Should().NotBeNull();
    }
}

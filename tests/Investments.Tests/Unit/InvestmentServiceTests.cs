using FluentAssertions;
using Investments.Application.DTOs;
using Investments.Application.Exceptions;
using Investments.Application.Services;
using Investments.Domain.Entities;
using Investments.Domain.Exceptions;
using Investments.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Investments.Tests.Unit;

public class InvestmentServiceTests
{
    private readonly Mock<IInvestmentRepository> _repo = new();
    private readonly Mock<IInvestmentTypeRepository> _types = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly InvestmentService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    private static readonly InvestmentType Acoes = TypeWithId(1, "Acoes");
    private static readonly InvestmentType RendaFixa = TypeWithId(2, "RendaFixa");
    private static readonly InvestmentType Fundos = TypeWithId(3, "Fundos");
    private static readonly InvestmentType Cripto = TypeWithId(5, "Cripto");

    public InvestmentServiceTests()
    {
        foreach (var t in new[] { Acoes, RendaFixa, Fundos, Cripto })
            _types.Setup(r => r.GetByNameAsync(t.Name, It.IsAny<CancellationToken>())).ReturnsAsync(t);

        _types.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<InvestmentType> { Acoes, RendaFixa, Fundos, Cripto });

        _sut = new InvestmentService(_repo.Object, _types.Object, _uow.Object, NullLogger<InvestmentService>.Instance);
    }

    /// <summary>O Id é gerado pelo banco, então o teste o injeta por reflexão.</summary>
    private static InvestmentType TypeWithId(int id, string name)
    {
        var type = new InvestmentType(name);
        typeof(InvestmentType).GetProperty(nameof(InvestmentType.Id))!
            .GetSetMethod(nonPublic: true)!.Invoke(type, new object[] { id });
        return type;
    }

    [Fact]
    public async Task Create_deve_persistir_e_commitar()
    {
        var request = new InvestmentRequest("Acoes", 1000, DateTime.UtcNow, "VALE3");

        var result = await _sut.CreateAsync(_userId, request);

        result.Amount.Should().Be(1000);
        result.Type.Should().Be("Acoes");
        result.TypeId.Should().Be(1);
        _repo.Verify(r => r.AddAsync(It.Is<Investment>(i => i.UserId == _userId), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_com_tipo_inexistente_deve_falhar_listando_os_disponiveis()
    {
        var act = () => _sut.CreateAsync(_userId, new InvestmentRequest("Fiis", 10, DateTime.UtcNow, null));

        (await act.Should().ThrowAsync<DomainException>())
            .WithMessage("*Fiis*").And.Message.Should().Contain("Acoes");
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_de_investimento_de_outro_usuario_deve_retornar_NotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Investment?)null);

        var act = () => _sut.UpdateAsync(_userId, Guid.NewGuid(),
            new InvestmentRequest("Fundos", 10, DateTime.UtcNow, null));

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_deve_remover_investimento_do_usuario()
    {
        var inv = new Investment(_userId, Cripto.Id, 50, DateTime.UtcNow, null);
        _repo.Setup(r => r.GetByIdAsync(inv.Id, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(inv);

        await _sut.DeleteAsync(_userId, inv.Id);

        _repo.Verify(r => r.Remove(inv), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_deve_retornar_apenas_investimentos_do_usuario()
    {
        _repo.Setup(r => r.GetByUserAsync(_userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Investment>
             {
                 new(_userId, Acoes.Id, 1, DateTime.UtcNow, null),
                 new(_userId, RendaFixa.Id, 2, DateTime.UtcNow, null)
             });

        var result = await _sut.ListAsync(_userId);

        result.Should().HaveCount(2);
    }
}

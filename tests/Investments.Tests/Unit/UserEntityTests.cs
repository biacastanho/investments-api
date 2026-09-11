using FluentAssertions;
using Investments.Application.Services;
using Investments.Domain.Entities;
using Investments.Domain.Exceptions;

namespace Investments.Tests.Unit;

public class UserEntityTests
{
    [Fact]
    public void Deve_criar_usuario_normalizando_email()
    {
        var user = new User("  Marcio ", "Marcio@Email.com", "hash");

        user.Name.Should().Be("Marcio");
        user.Email.Should().Be("marcio@email.com");
        user.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("", "a@b.com", "hash")]
    [InlineData("Nome", "email-invalido", "hash")]
    [InlineData("Nome", "a@b.com", "")]
    public void Nao_deve_criar_usuario_invalido(string name, string email, string hash)
    {
        var act = () => new User(name, email, hash);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Senha_deve_ser_armazenada_como_hash_bcrypt()
    {
        var hasher = new BcryptPasswordHasher();
        var hash = hasher.Hash("Senha@123");

        hash.Should().NotBe("Senha@123");
        hash.Should().StartWith("$2");
        hasher.Verify("Senha@123", hash).Should().BeTrue();
        hasher.Verify("outra", hash).Should().BeFalse();
    }
}

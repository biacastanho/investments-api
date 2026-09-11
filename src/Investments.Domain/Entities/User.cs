using Investments.Domain.Exceptions;

namespace Investments.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<Investment> _investments = new();
    public IReadOnlyCollection<Investment> Investments => _investments.AsReadOnly();

    private User() { }

    public User(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new DomainException("E-mail inválido.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Hash de senha é obrigatório.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }
}

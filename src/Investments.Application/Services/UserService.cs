using Investments.Application.DTOs;
using Investments.Application.Exceptions;
using Investments.Application.Interfaces;
using Investments.Domain.Entities;
using Investments.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Investments.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository users, IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens, ILogger<UserService> logger)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email, ct))
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");

        var user = new User(request.Name, email, _hasher.Hash(request.Password));
        await _users.AddAsync(user, ct);
        await _uow.CommitAsync(ct);

        _logger.LogInformation("Usuário {UserId} cadastrado com e-mail {Email}", user.Id, user.Email);
        return ToResponse(user);
    }

    public async Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login inválida para {Email}", request.Email);
            throw new UnauthorizedException("E-mail ou senha inválidos.");
        }

        var (token, expiresAt) = _tokens.Generate(user);
        _logger.LogInformation("Usuário {UserId} autenticado", user.Id);
        return new AuthResponse(token, expiresAt, ToResponse(user));
    }

    private static UserResponse ToResponse(User u) => new(u.Id, u.Name, u.Email, u.CreatedAt);
}

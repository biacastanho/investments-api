using Investments.Application.DTOs;
using Investments.Domain.Entities;

namespace Investments.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<AuthResponse> AuthenticateAsync(AuthRequest request, CancellationToken ct = default);
}

public interface IInvestmentTypeService
{
    Task<IReadOnlyList<InvestmentTypeResponse>> ListAsync(CancellationToken ct = default);
    Task<InvestmentTypeResponse> GetAsync(int id, CancellationToken ct = default);
    Task<InvestmentTypeResponse> CreateAsync(InvestmentTypeRequest request, CancellationToken ct = default);
    Task<InvestmentTypeResponse> UpdateAsync(int id, InvestmentTypeRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IInvestmentService
{
    Task<IReadOnlyList<InvestmentResponse>> ListAsync(Guid userId, CancellationToken ct = default);
    Task<InvestmentResponse> GetAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<InvestmentResponse> CreateAsync(Guid userId, InvestmentRequest request, CancellationToken ct = default);
    Task<InvestmentResponse> UpdateAsync(Guid userId, Guid id, InvestmentRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
}

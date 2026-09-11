using System.ComponentModel.DataAnnotations;

namespace Investments.Application.DTOs;

public record RegisterUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password);

public record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt);

public record AuthRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserResponse User);

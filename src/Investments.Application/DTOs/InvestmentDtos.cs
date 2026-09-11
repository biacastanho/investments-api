using System.ComponentModel.DataAnnotations;

namespace Investments.Application.DTOs;

public record InvestmentRequest(
    [Required] string Type,
    [Required, Range(0.01, double.MaxValue)] decimal Amount,
    [Required] DateTime? InvestedAt,
    [MaxLength(200)] string? Description);

public record InvestmentResponse(
    Guid Id,
    int TypeId,
    string Type,
    decimal Amount,
    DateTime InvestedAt,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record InvestmentTypeRequest(
    [Required, MaxLength(50)] string Name);

public record InvestmentTypeResponse(
    int Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

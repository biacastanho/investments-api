using System.Text.Json;
using Investments.Application.Exceptions;
using Investments.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Middlewares;

/// <summary>Converte exceções em respostas ProblemDetails (RFC 7807).</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                DomainException => (StatusCodes.Status400BadRequest, "Erro de validação"),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflito"),
                UnauthorizedException or UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Erro não tratado em {Path}", context.Request.Path);
            else
                _logger.LogWarning("{Title} em {Path}: {Message}", title, context.Request.Path, ex.Message);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == 500 ? "Ocorreu um erro inesperado." : ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}

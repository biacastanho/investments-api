using Investments.Application.DTOs;
using Investments.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Controllers;

[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    public AuthController(IUserService users) => _users = users;

    /// <summary>Cadastra um novo usuário.</summary>
    [HttpPost("/users")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await _users.RegisterAsync(request, ct);
        return Created($"/users/{user.Id}", user);
    }

    /// <summary>Autentica o usuário e retorna um token JWT.</summary>
    [HttpPost("/auth")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequest request, CancellationToken ct)
        => Ok(await _users.AuthenticateAsync(request, ct));
}

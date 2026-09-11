using System.Security.Claims;
using Investments.Application.DTOs;
using Investments.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Controllers;

[ApiController]
[Authorize]
[Route("/investments")]
[Produces("application/json")]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _service;
    public InvestmentsController(IInvestmentService service) => _service = service;

    // O id vem da claim "sub" do JWT; qualquer token sem um Guid valido resulta em 401, nunca em 500.
    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Token sem identificador de usuario valido.");

    /// <summary>Lista os investimentos do usuário autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvestmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _service.ListAsync(CurrentUserId, ct));

    /// <summary>Obtém um investimento específico do usuário autenticado.</summary>
    [HttpGet("{id:guid}", Name = "GetInvestment")]
    [ProducesResponseType(typeof(InvestmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _service.GetAsync(CurrentUserId, id, ct));

    /// <summary>Adiciona um novo investimento para o usuário autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvestmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] InvestmentRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um investimento do usuário autenticado.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InvestmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] InvestmentRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(CurrentUserId, id, request, ct));

    /// <summary>Remove um investimento do usuário autenticado.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}

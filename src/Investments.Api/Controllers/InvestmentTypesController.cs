using Investments.Application.DTOs;
using Investments.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investments.Api.Controllers;

/// <summary>
/// Manutenção dos tipos de investimento. São dados de referência compartilhados:
/// qualquer usuário autenticado consulta e mantém a mesma lista.
/// </summary>
[ApiController]
[Authorize]
[Route("/investment-types")]
[Produces("application/json")]
public class InvestmentTypesController : ControllerBase
{
    private readonly IInvestmentTypeService _service;
    public InvestmentTypesController(IInvestmentTypeService service) => _service = service;

    /// <summary>Lista os tipos de investimento disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvestmentTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _service.ListAsync(ct));

    /// <summary>Obtém um tipo de investimento pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InvestmentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetAsync(id, ct));

    /// <summary>Cadastra um novo tipo de investimento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvestmentTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] InvestmentTypeRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Renomeia um tipo de investimento.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(InvestmentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] InvestmentTypeRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    /// <summary>Remove um tipo de investimento que ainda não esteja em uso.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}

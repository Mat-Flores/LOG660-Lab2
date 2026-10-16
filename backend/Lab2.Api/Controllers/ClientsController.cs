using System.Security.Claims;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service) => _service = service;

    // GET api/clients/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(int id, CancellationToken ct)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) != id.ToString())
            return Forbid();

        var client = await _service.GetByIdAsync(id, ct);

        if (client is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Client introuvable",
                Detail = $"Aucun client avec l'id {id}."
            });
        }

        return Ok(client);
    }
}

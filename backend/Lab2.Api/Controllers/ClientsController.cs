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

    // GET api/clients/me : dossier du client connecté (restaure la session côté frontend).
    [HttpGet("me")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientDto>> GetMe(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var client = await _service.GetByIdAsync(id, ct);

        return client is null ? Unauthorized() : Ok(client);
    }
}

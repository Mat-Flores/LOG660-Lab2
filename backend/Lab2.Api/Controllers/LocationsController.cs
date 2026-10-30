using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service) => _service = service;

    // POST api/locations { "idUtilisateur": 997690, "idFilm": 74958 } : loue une copie (cas 4).
    [HttpPost]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocationDto>> Louer(NouvelleLocationDto demande, CancellationToken ct)
    {
        try
        {
            var location = await _service.LouerAsync(demande.IdUtilisateur, demande.IdFilm, ct);
            return StatusCode(StatusCodes.Status201Created, location);
        }
        catch (RegleAffairesException e)
        {
            return Problem(statusCode: e.StatusCode, title: "Location refusée", detail: e.Message);
        }
    }
}

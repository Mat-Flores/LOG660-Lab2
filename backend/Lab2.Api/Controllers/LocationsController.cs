using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

// Cas 4 : idUtilisateur vient du ClientDto reçu au login ; le service vérifie que c'est bien un client.
[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service) => _service = service;

    // POST api/locations { "idUtilisateur": 997690, "idFilm": 74958 }
    [HttpPost]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocationDto>> Louer(NouvelleLocationDto demande, CancellationToken ct)
    {
        try
        {
            var location = await _service.LouerAsync(demande.IdUtilisateur, demande.IdFilm, ct);
            return StatusCode(StatusCodes.Status201Created, location);
        }
        catch (RegleLocationException ex)
        {
            return Refuser(ex);
        }
    }

    private ObjectResult Refuser(RegleLocationException ex)
        => Problem(title: "Location refusée", detail: ex.Message, statusCode: ex.Motif switch
        {
            MotifRefusLocation.NonClient => StatusCodes.Status403Forbidden,
            MotifRefusLocation.FilmIntrouvable => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status409Conflict
        });
}

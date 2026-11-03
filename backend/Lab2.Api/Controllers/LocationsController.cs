using System.Security.Claims;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Louer(NouvelleLocationDto demande, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idClient))
            return Unauthorized();

        if (demande.IdUtilisateur != idClient)
            return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Location refusée",
                detail: "Vous pouvez uniquement louer avec votre propre compte.");

        try
        {
            var location = await _service.LouerAsync(idClient, demande.IdFilm, ct);
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

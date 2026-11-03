using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

// Cas 3 : recherche et consultation du catalogue.
[ApiController]
[Route("api/films")]
public class FilmsController : ControllerBase
{
    private readonly ICatalogueService _service;

    public FilmsController(ICatalogueService service) => _service = service;

    // GET api/films?titre=..&anneeMin=..&anneeMax=..&pays=..&langue=..&genre=..&realisateur=..&acteur=..
    // Liste vide = aucun film ne correspond (le frontend affiche le message d'erreur).
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FilmResumeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<FilmResumeDto>>> Rechercher(
        [FromQuery] RechercheFilmCriteres criteres, CancellationToken ct)
    {
        return Ok(await _service.RechercherFilmsAsync(criteres, ct));
    }

    // GET api/films/criteres : genres, pays et langues pour les listes déroulantes.
    [HttpGet("criteres")]
    [ProducesResponseType(typeof(CriteresRechercheDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CriteresRechercheDto>> GetCriteres(CancellationToken ct)
        => Ok(await _service.GetCriteresAsync(ct));

    // GET api/films/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FilmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FilmDetailDto>> GetById(int id, CancellationToken ct)
    {
        var film = await _service.GetFilmAsync(id, ct);

        return film is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Film introuvable", detail: $"Aucun film avec l'id {id}.")
            : Ok(film);
    }
}

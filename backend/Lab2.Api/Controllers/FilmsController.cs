using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/films")]
public class FilmsController : ControllerBase
{
    private readonly ICatalogueService _service;

    public FilmsController(ICatalogueService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FilmResumeDto>>> Rechercher(
        [FromQuery] RechercheFilms recherche, CancellationToken ct)
        => Ok(await _service.RechercherFilmsAsync(recherche, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FilmDto>> GetById(int id, CancellationToken ct)
    {
        var film = await _service.GetFilmByIdAsync(id, ct);

        if (film is null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Film introuvable",
                Detail = $"Aucun film avec l'id {id}."
            });

        return Ok(film);
    }
}

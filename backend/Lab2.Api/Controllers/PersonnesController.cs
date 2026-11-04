using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Route("api/personnes")]
public class PersonnesController : ControllerBase
{
    private readonly ICatalogueService _service;

    public PersonnesController(ICatalogueService service) => _service = service;

    // GET api/personnes/{id} : fiche d'un réalisateur ou d'un acteur.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PersonneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonneDto>> GetById(int id, CancellationToken ct)
    {
        var personne = await _service.GetPersonneAsync(id, ct);

        return personne is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Personne introuvable", detail: $"Aucune personne avec l'id {id}.")
            : Ok(personne);
    }
}

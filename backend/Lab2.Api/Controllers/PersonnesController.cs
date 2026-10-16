using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/personnes")]
public class PersonnesController : ControllerBase
{
    private readonly ICatalogueService _service;

    public PersonnesController(ICatalogueService service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonneDto>> GetById(int id, CancellationToken ct)
    {
        var personne = await _service.GetPersonneByIdAsync(id, ct);

        if (personne is null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Personne introuvable",
                Detail = $"Aucune personne avec l'id {id}."
            });

        return Ok(personne);
    }
}

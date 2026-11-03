using Lab2.Api.Dtos;
using Lab2.Infrastructure.Repositories;

namespace Lab2.Api.Services;

// Façade du cas 3 : recherche et consultation des films et des personnes.
public interface ICatalogueService
{
    Task<IReadOnlyList<FilmResumeDto>> RechercherFilmsAsync(RechercheFilmCriteres criteres, CancellationToken ct = default);
    Task<FilmDetailDto?> GetFilmAsync(int idFilm, CancellationToken ct = default);
    Task<PersonneDto?> GetPersonneAsync(int idPersonne, CancellationToken ct = default);
    Task<CriteresRechercheDto> GetCriteresAsync(CancellationToken ct = default);
}

using Lab2.Api.Dtos;
using Lab2.Infrastructure.Models;

namespace Lab2.Api.Services;

public interface ICatalogueService
{
    Task<IReadOnlyList<FilmResumeDto>> RechercherFilmsAsync(RechercheFilms recherche, CancellationToken ct = default);
    Task<FilmDto?> GetFilmByIdAsync(int id, CancellationToken ct = default);
    Task<PersonneDto?> GetPersonneByIdAsync(int id, CancellationToken ct = default);
}

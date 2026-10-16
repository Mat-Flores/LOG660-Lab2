using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Models;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface ICatalogueRepository
{
    Task<IReadOnlyList<Film>> RechercherFilmsAsync(RechercheFilms recherche, CancellationToken ct = default);
    Task<Film?> GetFilmByIdAsync(int id, CancellationToken ct = default);
    Task<Personne?> GetPersonneByIdAsync(int id, CancellationToken ct = default);
}

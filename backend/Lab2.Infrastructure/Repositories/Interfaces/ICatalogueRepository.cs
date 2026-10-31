using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface ICatalogueRepository
{
    Task<IReadOnlyList<Film>> RechercherFilmsAsync(RechercheFilmCriteres criteres, CancellationToken ct = default);
    Task<Film?> GetFilmAsync(int idFilm, CancellationToken ct = default);
    Task<Personne?> GetPersonneAsync(int idPersonne, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPaysAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetLanguesAsync(CancellationToken ct = default);
}

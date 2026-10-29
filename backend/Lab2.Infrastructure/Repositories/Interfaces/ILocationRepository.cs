using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface ILocationRepository
{
    Task<bool> FilmExisteAsync(int idFilm, CancellationToken ct = default);
    Task<string?> TrouverCopieDisponibleAsync(int idFilm, CancellationToken ct = default);
    Task<int> CompterLocationsEnCoursAsync(int idUtilisateur, CancellationToken ct = default);
    Task AjouterAsync(Location location, CancellationToken ct = default);
}

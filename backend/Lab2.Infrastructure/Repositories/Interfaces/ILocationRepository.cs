using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface ILocationRepository
{
    Task<IDbContextTransaction> CommencerTransactionAsync(CancellationToken ct = default);
    Task<Client?> GetClientAsync(int id, CancellationToken ct = default);
    Task<Film?> GetFilmPourLocationAsync(int id, CancellationToken ct = default);
    Task<int> CompterLocationsActivesAsync(int idClient, CancellationToken ct = default);
    Task<Copie?> GetCopieDisponibleAsync(int idFilm, CancellationToken ct = default);
    Task AjouterAsync(Location location, CancellationToken ct = default);
}

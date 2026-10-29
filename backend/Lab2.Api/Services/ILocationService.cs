using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

// Façade du cas 4 : location d'un film.
public interface ILocationService
{
    /// <exception cref="Lab2.Infrastructure.Exceptions.RegleAffairesException">
    /// Film inexistant (404), aucune copie disponible ou quota du forfait atteint (409).
    /// </exception>
    Task<LocationDto> LouerAsync(int idUtilisateur, int idFilm, CancellationToken ct = default);
}

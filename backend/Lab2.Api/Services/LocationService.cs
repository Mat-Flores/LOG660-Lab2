using Lab2.Api.Dtos;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Exceptions;
using Lab2.Infrastructure.Repositories.Interfaces;

namespace Lab2.Api.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locations;
    private readonly IClientRepository _clients;

    public LocationService(ILocationRepository locations, IClientRepository clients)
    {
        _locations = locations;
        _clients = clients;
    }

    // Les règles sont vérifiées ici pour des messages clairs ; le trigger de la BD les garantit.
    public async Task<LocationDto> LouerAsync(int idUtilisateur, int idFilm, CancellationToken ct = default)
    {
        var client = await _clients.GetByIdAsync(idUtilisateur, ct)
            ?? throw new RegleAffairesException("Client introuvable.", StatusCodes.Status404NotFound);

        if (!await _locations.FilmExisteAsync(idFilm, ct))
            throw new RegleAffairesException($"Aucun film avec l'id {idFilm}.", StatusCodes.Status404NotFound);

        var enCours = await _locations.CompterLocationsEnCoursAsync(idUtilisateur, ct);
        if (enCours >= client.Forfait.LocationsMax)
            throw new RegleAffairesException(
                $"Votre forfait permet {client.Forfait.LocationsMax} location(s) à la fois et vous en avez {enCours} : retournez d'abord un film.");

        var codeCopie = await _locations.TrouverCopieDisponibleAsync(idFilm, ct)
            ?? throw new RegleAffairesException("Aucune copie de ce film n'est disponible en inventaire.");

        var location = new Location
        {
            IdUtilisateur = idUtilisateur,
            CodeCopie = codeCopie,
            // Colonne DATE sans fuseau : Unspecified évite un décalage horaire dans le JSON.
            DateLocation = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified)
        };
        await _locations.AjouterAsync(location, ct);

        return new LocationDto
        {
            IdLocation = location.IdLocation,
            IdFilm = idFilm,
            CodeCopie = codeCopie,
            DateLocation = location.DateLocation,
            DateRetourMax = client.Forfait.DureeMaxJours is int jours ? location.DateLocation.AddDays(jours) : null
        };
    }
}

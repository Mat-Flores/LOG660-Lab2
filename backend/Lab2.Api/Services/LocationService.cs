using Lab2.Api.Dtos;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Models;
using Lab2.Infrastructure.Repositories.Interfaces;

namespace Lab2.Api.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _repository;

    public LocationService(ILocationRepository repository) => _repository = repository;

    public async Task<LocationDto> LouerAsync(int idClient, int idFilm, CancellationToken ct = default)
    {
        await using var transaction = await _repository.CommencerTransactionAsync(ct);

        // Toujours verrouiller le client avant le film : quota et disponibilité restent cohérents.
        var client = await _repository.GetClientAsync(idClient, verrouiller: true, ct);
        var forfait = ExigerForfait(client);
        var film = await _repository.GetFilmPourLocationAsync(idFilm, ct)
            ?? throw new RegleLocationException(MotifRefusLocation.FilmIntrouvable, "Ce film est introuvable.");

        if (await _repository.CompterLocationsActivesAsync(idClient, ct) >= forfait.LocationsMax)
            throw new RegleLocationException(MotifRefusLocation.QuotaAtteint,
                "La limite de locations de votre forfait est atteinte.");

        var copie = await _repository.GetCopieDisponibleAsync(idFilm, ct)
            ?? throw new RegleLocationException(MotifRefusLocation.AucuneCopie,
                "Aucune copie de ce film n'est disponible.");

        copie.Film = film;
        var location = new Location
        {
            IdUtilisateur = idClient,
            Client = client!,
            CodeCopie = copie.CodeCopie,
            Copie = copie,
            DateLocation = DateTime.Today
        };

        await _repository.AjouterAsync(location, ct);
        await transaction.CommitAsync(ct);
        return Convertir(location, forfait);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsActivesAsync(int idClient, CancellationToken ct = default)
    {
        var client = await _repository.GetClientAsync(idClient, verrouiller: false, ct);
        var forfait = ExigerForfait(client);
        var locations = await _repository.GetLocationsActivesAsync(idClient, ct);
        return locations.Select(l => Convertir(l, forfait)).ToList();
    }

    private static Forfait ExigerForfait(Client? client)
    {
        if (client is null)
            throw new RegleLocationException(MotifRefusLocation.NonClient,
                "La location est réservée aux clients.");

        if (client.Forfait is null || client.Forfait.LocationsMax <= 0)
            throw new RegleLocationException(MotifRefusLocation.ForfaitInvalide,
                "Le client doit avoir un forfait valide pour louer un film.");

        return client.Forfait;
    }

    private static LocationDto Convertir(Location location, Forfait forfait) => new()
    {
        IdLocation = location.IdLocation,
        IdFilm = location.Copie.IdFilm,
        Titre = location.Copie.Film.Titre,
        CodeCopie = location.CodeCopie,
        DateLocation = location.DateLocation,
        DateRetourPrevue = forfait.DureeMaxJours.HasValue
            ? location.DateLocation.AddDays(forfait.DureeMaxJours.Value)
            : null
    };
}

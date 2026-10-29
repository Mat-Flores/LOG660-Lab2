using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

public interface ILocationService
{
    Task<LocationDto> LouerAsync(int idClient, int idFilm, CancellationToken ct = default);
    Task<IReadOnlyList<LocationDto>> GetLocationsActivesAsync(int idClient, CancellationToken ct = default);
}

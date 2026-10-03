using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

public interface IClientService
{
    Task<ClientDto?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default);
}

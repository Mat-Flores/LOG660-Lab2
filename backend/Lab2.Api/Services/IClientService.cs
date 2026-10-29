using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

public interface IClientService
{
    Task<ClientDto?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default);

    // Cas 2 : null si le courriel est inconnu ou le mot de passe incorrect.
    Task<ClientDto?> AuthentifierAsync(string courriel, string motDePasse, CancellationToken ct = default);
}

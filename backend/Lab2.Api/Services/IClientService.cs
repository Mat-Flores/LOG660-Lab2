using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

public interface IClientService
{
    // Cas 2 : null si le courriel est inconnu ou le mot de passe incorrect.
    Task<ClientDto?> AuthentifierAsync(string courriel, string motDePasse, CancellationToken ct = default);

    // Vrai uniquement si le compte et le mot de passe correspondent à un employé.
    // Les employés ne sont pas autorisés à utiliser la connexion client de Webflix.
    Task<bool> EmployeAuthentifieAsync(string courriel, string motDePasse, CancellationToken ct = default);
}

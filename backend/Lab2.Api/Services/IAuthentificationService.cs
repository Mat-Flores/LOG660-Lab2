using Lab2.Api.Dtos;

namespace Lab2.Api.Services;

public interface IAuthentificationService
{
    Task<UtilisateurDto?> ConnecterAsync(ConnexionDto connexion, CancellationToken ct = default);
}

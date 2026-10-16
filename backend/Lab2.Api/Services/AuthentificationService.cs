using Lab2.Api.Dtos;
using Lab2.Infrastructure.Repositories.Interfaces;

namespace Lab2.Api.Services;

public class AuthentificationService : IAuthentificationService
{
    private readonly IUtilisateurRepository _repository;

    public AuthentificationService(IUtilisateurRepository repository) => _repository = repository;

    public async Task<UtilisateurDto?> ConnecterAsync(ConnexionDto connexion, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(connexion.Courriel)
            || string.IsNullOrWhiteSpace(connexion.MotDePasse))
            return null;

        var utilisateur = await _repository.GetByCourrielAsync(connexion.Courriel.Trim(), ct);

        // On conserve le format des mots de passe déjà stockés au labo 1.
        if (utilisateur is null
            || !string.Equals(utilisateur.MotDePasse, connexion.MotDePasse, StringComparison.Ordinal))
            return null;

        return new UtilisateurDto
        {
            IdUtilisateur = utilisateur.IdUtilisateur,
            NomFamille = utilisateur.NomFamille,
            Prenom = utilisateur.Prenom,
            Courriel = utilisateur.Courriel
        };
    }
}

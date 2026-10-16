using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface IUtilisateurRepository
{
    Task<Utilisateur?> GetByCourrielAsync(string courriel, CancellationToken ct = default);
}

using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByCourrielAsync(string courriel, CancellationToken ct = default);

    // Recherche d'un employé pour afficher un message explicite si ce type
    // de compte tente d'utiliser la connexion réservée aux clients.
    Task<Employe?> GetEmployeByCourrielAsync(string courriel, CancellationToken ct = default);
}

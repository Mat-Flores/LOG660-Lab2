using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default);
    Task<Client?> GetByCourrielAsync(string courriel, CancellationToken ct = default);
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default);
    Task<bool> CourrielExisteAsync(string courriel, CancellationToken ct = default);
}

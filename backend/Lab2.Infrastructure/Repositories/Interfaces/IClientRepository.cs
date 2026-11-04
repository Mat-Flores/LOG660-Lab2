using Lab2.Infrastructure.Entities;

namespace Lab2.Infrastructure.Repositories.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByCourrielAsync(string courriel, CancellationToken ct = default);

    Task<Employe?> GetEmployeByCourrielAsync(string courriel, CancellationToken ct = default);
}

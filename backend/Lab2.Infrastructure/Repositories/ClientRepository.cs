using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context) => _context = context;

    // Lecture seule : client + infos personnelles, adresse, forfait et carte.
    private IQueryable<Client> ClientsComplets() =>
        _context.Clients
            .AsNoTracking()
            .Include(c => c.Utilisateur).ThenInclude(u => u.Adresse)
            .Include(c => c.Forfait)
            .Include(c => c.CarteCredit);

    public Task<Client?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default)
        => ClientsComplets().FirstOrDefaultAsync(c => c.IdUtilisateur == idUtilisateur, ct);

    public Task<Client?> GetByCourrielAsync(string courriel, CancellationToken ct = default)
        => ClientsComplets().FirstOrDefaultAsync(c => c.Utilisateur.Courriel == courriel, ct);

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default)
        => await ClientsComplets()
            .OrderBy(c => c.Utilisateur.NomFamille)
            .ThenBy(c => c.Utilisateur.Prenom)
            .ToListAsync(ct);

    public Task<bool> CourrielExisteAsync(string courriel, CancellationToken ct = default)
        => _context.Utilisateurs.AnyAsync(u => u.Courriel == courriel, ct);
}

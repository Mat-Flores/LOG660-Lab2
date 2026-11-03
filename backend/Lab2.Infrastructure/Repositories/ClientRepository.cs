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
            .Include(c => c.Adresse)
            .Include(c => c.Forfait)
            .Include(c => c.CarteCredit);

    public Task<Client?> GetByCourrielAsync(string courriel, CancellationToken ct = default)
        => ClientsComplets().FirstOrDefaultAsync(c => c.Courriel == courriel, ct);

}

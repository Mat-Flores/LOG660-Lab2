using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Repositories;

public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly AppDbContext _context;

    public UtilisateurRepository(AppDbContext context) => _context = context;

    public Task<Utilisateur?> GetByCourrielAsync(string courriel, CancellationToken ct = default)
        => _context.Utilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Courriel == courriel, ct);
}

using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Exceptions;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Lab2.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context) => _context = context;

    public Task<bool> FilmExisteAsync(int idFilm, CancellationToken ct = default)
        => _context.Films.AnyAsync(f => f.IdFilm == idFilm, ct);

    // Une copie est disponible si elle n'a aucune location en cours (DATERETOUR null).
    public Task<string?> TrouverCopieDisponibleAsync(int idFilm, CancellationToken ct = default)
        => _context.Copies
            .Where(c => c.IdFilm == idFilm && !c.Locations.Any(l => l.DateRetour == null))
            .OrderBy(c => c.CodeCopie)
            .Select(c => c.CodeCopie)
            .FirstOrDefaultAsync(ct);

    public Task<int> CompterLocationsEnCoursAsync(int idUtilisateur, CancellationToken ct = default)
        => _context.Locations.CountAsync(l => l.IdUtilisateur == idUtilisateur && l.DateRetour == null, ct);

    // TRG_LOCATION_REGLES (labo 1) reste le garde-fou en cas de locations simultanées.
    public async Task AjouterAsync(Location location, CancellationToken ct = default)
    {
        _context.Locations.Add(location);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.InnerException is OracleException { Number: 20003 or 20004 } oracle)
        {
            throw new RegleAffairesException(oracle.Number == 20003
                ? "Cette copie vient d'être louée par un autre client. Veuillez réessayer."
                : "Votre forfait ne permet pas de nouvelle location : retournez d'abord un film.");
        }
    }
}

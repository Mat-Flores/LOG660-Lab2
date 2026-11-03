using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Models;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Oracle.ManagedDataAccess.Client;

namespace Lab2.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context) => _context = context;

    public Task<IDbContextTransaction> CommencerTransactionAsync(CancellationToken ct = default)
        => _context.Database.BeginTransactionAsync(ct);

    public async Task<Client?> GetClientAsync(int id, CancellationToken ct = default)
    {
        if (_context.Database.IsOracle())
            await VerrouillerAsync($"SELECT IDUTILISATEUR AS \"Value\" FROM CLIENT WHERE IDUTILISATEUR = {id} FOR UPDATE WAIT 5", ct);
        var client = await _context.Clients.SingleOrDefaultAsync(c => c.IdUtilisateur == id, ct);

        if (client is not null)
            await _context.Entry(client).Reference(c => c.Forfait).LoadAsync(ct);

        return client;
    }

    public async Task<Film?> GetFilmPourLocationAsync(int id, CancellationToken ct = default)
    {
        if (_context.Database.IsOracle())
            await VerrouillerAsync($"SELECT IDFILM AS \"Value\" FROM FILM WHERE IDFILM = {id} FOR UPDATE WAIT 5", ct);
        return await _context.Films.SingleOrDefaultAsync(f => f.IdFilm == id, ct);
    }

    public Task<int> CompterLocationsActivesAsync(int idClient, CancellationToken ct = default)
        => _context.Locations.CountAsync(l => l.IdUtilisateur == idClient && l.DateRetour == null, ct);

    public Task<Copie?> GetCopieDisponibleAsync(int idFilm, CancellationToken ct = default)
        => _context.Copies
            .Where(c => c.IdFilm == idFilm && !c.Locations.Any(l => l.DateRetour == null))
            .OrderBy(c => c.CodeCopie)
            .FirstOrDefaultAsync(ct);

    public async Task AjouterAsync(Location location, CancellationToken ct = default)
    {
        _context.Locations.Add(location);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 20003 or 20004 })
        {
            // Les triggers du labo 1 restent une seconde validation des règles de location.
            throw new RegleLocationException(MotifRefusLocation.DemandeConcurrente,
                "La disponibilité ou le quota a changé. Veuillez réessayer.");
        }
    }

    private async Task VerrouillerAsync(FormattableString requete, CancellationToken ct)
    {
        try
        {
            // Exécution directe : ajouter un filtre LINQ ici pourrait envelopper le FOR UPDATE.
            await _context.Database.SqlQuery<int>(requete).ToListAsync(ct);
        }
        catch (OracleException ex) when (ex.Number is 54 or 60 or 30006)
        {
            throw new RegleLocationException(MotifRefusLocation.DemandeConcurrente,
                "Une autre location est en cours de traitement. Veuillez réessayer.");
        }
    }
}

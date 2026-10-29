using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Repositories;

public class CatalogueRepository : ICatalogueRepository
{
    private readonly AppDbContext _context;

    public CatalogueRepository(AppDbContext context) => _context = context;

    // Chaque critère ajoute un WHERE : la requête finale est un seul SELECT exécuté par Oracle.
    public async Task<IReadOnlyList<Film>> RechercherFilmsAsync(RechercheFilmCriteres criteres, CancellationToken ct = default)
    {
        var films = _context.Films.AsNoTracking();

        foreach (var titre in NonVides(criteres.Titre))
            foreach (var mot in Mots(titre))
                films = films.Where(f => f.Titre.ToUpper().Contains(mot));

        if (criteres.AnneeMin is int anneeMin)
            films = films.Where(f => f.AnneeSortie >= anneeMin);
        if (criteres.AnneeMax is int anneeMax)
            films = films.Where(f => f.AnneeSortie <= anneeMax);

        if (!string.IsNullOrWhiteSpace(criteres.Langue))
        {
            var langue = criteres.Langue.Trim();
            films = films.Where(f => f.LangueOriginale == langue);
        }

        foreach (var pays in NonVides(criteres.Pays))
            films = films.Where(f => f.FilmPays.Any(p => p.NomPays == pays));

        foreach (var genre in NonVides(criteres.Genre))
            films = films.Where(f => f.FilmGenres.Any(g => g.NomGenre == genre));

        foreach (var nom in NonVides(criteres.Realisateur))
        {
            var realisateurs = PersonnesNommees(nom);
            films = films.Where(f => f.IdRealisateur != null && realisateurs.Contains(f.IdRealisateur.Value));
        }

        // Un même acteur doit contenir tous les mots ; deux critères acteur = deux acteurs.
        foreach (var nom in NonVides(criteres.Acteur))
        {
            var acteurs = PersonnesNommees(nom);
            films = films.Where(f => f.Interpretations.Any(i => acteurs.Contains(i.IdPersonne)));
        }

        return await films.OrderBy(f => f.Titre).ThenBy(f => f.AnneeSortie).ToListAsync(ct);
    }

    // Fiche complète (cas 3). Requêtes séparées pour éviter le produit cartésien des collections.
    public Task<Film?> GetFilmAsync(int idFilm, CancellationToken ct = default)
        => _context.Films
            .AsNoTracking()
            .AsSplitQuery()
            .Include(f => f.Realisateur)
            .Include(f => f.FilmPays)
            .Include(f => f.FilmGenres)
            .Include(f => f.Scenaristes).ThenInclude(s => s.Personne)
            .Include(f => f.Interpretations).ThenInclude(i => i.Personne)
            .Include(f => f.BandesAnnonces)
            .FirstOrDefaultAsync(f => f.IdFilm == idFilm, ct);

    public Task<int> CompterCopiesDisponiblesAsync(int idFilm, CancellationToken ct = default)
        => _context.Copies.CountAsync(
            c => c.IdFilm == idFilm && !c.Locations.Any(l => l.DateRetour == null), ct);

    public Task<Personne?> GetPersonneAsync(int idPersonne, CancellationToken ct = default)
        => _context.Personnes.AsNoTracking().FirstOrDefaultAsync(p => p.IdPersonne == idPersonne, ct);

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default)
        => await _context.Genres.Select(g => g.Nom).OrderBy(n => n).ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetPaysAsync(CancellationToken ct = default)
        => await _context.ListePays.Select(p => p.Nom).OrderBy(n => n).ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetLanguesAsync(CancellationToken ct = default)
        => await _context.Films
            .Where(f => f.LangueOriginale != null)
            .Select(f => f.LangueOriginale!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(ct);

    // Sous-requête : ids des personnes dont le nom contient tous les mots, dans n'importe quel ordre.
    private IQueryable<int> PersonnesNommees(string nom)
    {
        var personnes = _context.Personnes.AsQueryable();
        foreach (var mot in Mots(nom))
            personnes = personnes.Where(p => p.Nom.ToUpper().Contains(mot));
        return personnes.Select(p => p.IdPersonne);
    }

    private static IEnumerable<string> NonVides(IEnumerable<string> valeurs)
        => valeurs.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim());

    private static string[] Mots(string texte)
        => texte.ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
}

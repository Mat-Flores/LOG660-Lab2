using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Models;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Repositories;

public class CatalogueRepository : ICatalogueRepository
{
    private readonly AppDbContext _context;

    public CatalogueRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Film>> RechercherFilmsAsync(RechercheFilms recherche, CancellationToken ct = default)
    {
        var films = _context.Films.AsNoTracking();

        // Tous les mots doivent être présents, sans imposer leur ordre dans le titre.
        foreach (var mot in Mots(recherche.Titre))
            films = films.Where(f => f.Titre != null && f.Titre.ToLower().Contains(mot));

        if (!string.IsNullOrWhiteSpace(recherche.Acteur))
        {
            var acteurs = IdentifiantsPersonnesParNom(recherche.Acteur);
            films = films.Where(f => f.Interpretations.Any(i => acteurs.Contains(i.IdPersonne)));
        }

        if (!string.IsNullOrWhiteSpace(recherche.Realisateur))
        {
            var realisateurs = IdentifiantsPersonnesParNom(recherche.Realisateur);
            films = films.Where(f => f.IdRealisateur.HasValue && realisateurs.Contains(f.IdRealisateur.Value));
        }

        if (!string.IsNullOrWhiteSpace(recherche.Genre))
        {
            var genre = recherche.Genre.Trim().ToLowerInvariant();
            films = films.Where(f => f.FilmGenres.Any(g => g.NomGenre.ToLower() == genre));
        }

        if (!string.IsNullOrWhiteSpace(recherche.Pays))
        {
            var pays = recherche.Pays.Trim().ToLowerInvariant();
            films = films.Where(f => f.FilmPays.Any(p => p.NomPays.ToLower() == pays));
        }

        if (!string.IsNullOrWhiteSpace(recherche.Langue))
        {
            var langue = recherche.Langue.Trim().ToLowerInvariant();
            films = films.Where(f => f.LangueOriginale != null && f.LangueOriginale.ToLower() == langue);
        }

        if (recherche.AnneeMin.HasValue)
            films = films.Where(f => f.AnneeSortie >= recherche.AnneeMin.Value);

        if (recherche.AnneeMax.HasValue)
            films = films.Where(f => f.AnneeSortie <= recherche.AnneeMax.Value);

        return await films.OrderBy(f => f.Titre).ThenBy(f => f.IdFilm).ToListAsync(ct);
    }

    public Task<Film?> GetFilmByIdAsync(int id, CancellationToken ct = default)
        => _context.Films
            .AsNoTracking()
            // Des requêtes séparées évitent de multiplier les lignes entre les collections.
            .AsSplitQuery()
            .Include(f => f.Realisateur)
            .Include(f => f.Interpretations).ThenInclude(i => i.Personne)
            .Include(f => f.Scenaristes).ThenInclude(s => s.Personne)
            .Include(f => f.FilmGenres)
            .Include(f => f.FilmPays)
            .Include(f => f.BandesAnnonces)
            .FirstOrDefaultAsync(f => f.IdFilm == id, ct);

    public Task<Personne?> GetPersonneByIdAsync(int id, CancellationToken ct = default)
        => _context.Personnes.AsNoTracking().FirstOrDefaultAsync(p => p.IdPersonne == id, ct);

    private IQueryable<int> IdentifiantsPersonnesParNom(string nom)
    {
        var personnes = _context.Personnes.AsNoTracking();

        // Les mots doivent appartenir au nom d'une même personne.
        foreach (var mot in Mots(nom))
            personnes = personnes.Where(p => p.Nom != null && p.Nom.ToLower().Contains(mot));

        return personnes.Select(p => p.IdPersonne);
    }

    private static string[] Mots(string? texte)
        => (texte ?? string.Empty).ToLowerInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
}

namespace Lab2.Infrastructure.Repositories;

// Critères du cas 3, liés directement à la query string (?acteur=a&acteur=b).
// Chaque critère peut être répété : tous doivent être satisfaits.
public class RechercheFilmCriteres
{
    public List<string> Titre { get; set; } = new();
    public int? AnneeMin { get; set; }
    public int? AnneeMax { get; set; }
    public List<string> Pays { get; set; } = new();
    public string? Langue { get; set; }
    public List<string> Genre { get; set; } = new();
    public List<string> Realisateur { get; set; } = new();
    public List<string> Acteur { get; set; } = new();
}

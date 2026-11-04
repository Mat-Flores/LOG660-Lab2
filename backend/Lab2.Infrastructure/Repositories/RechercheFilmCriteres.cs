using System.ComponentModel.DataAnnotations;

namespace Lab2.Infrastructure.Repositories;

// Les critères répétés (?acteur=a&acteur=b) doivent tous être satisfaits.
public class RechercheFilmCriteres : IValidatableObject
{
    public List<string> Titre { get; set; } = new();
    [Range(1, 9999)]
    public int? AnneeMin { get; set; }
    [Range(1, 9999)]
    public int? AnneeMax { get; set; }
    public List<string> Pays { get; set; } = new();
    [StringLength(30)]
    public string? Langue { get; set; }
    public List<string> Genre { get; set; } = new();
    public List<string> Realisateur { get; set; } = new();
    public List<string> Acteur { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnneeMin > AnneeMax)
            yield return new ValidationResult("L'année minimale doit précéder l'année maximale.", new[] { nameof(AnneeMin) });

        foreach (var (nom, valeurs, longueur) in new[]
        {
            (nameof(Titre), Titre, 120), (nameof(Pays), Pays, 30),
            (nameof(Genre), Genre, 20), (nameof(Realisateur), Realisateur, 60),
            (nameof(Acteur), Acteur, 60)
        })
            if (valeurs.Any(v => v?.Length > longueur))
                yield return new ValidationResult($"Chaque critère {nom} est limité à {longueur} caractères.", new[] { nom });
    }
}

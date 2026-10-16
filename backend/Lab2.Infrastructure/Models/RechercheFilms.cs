using System.ComponentModel.DataAnnotations;

namespace Lab2.Infrastructure.Models;

public class RechercheFilms : IValidatableObject
{
    [StringLength(120)]
    public string? Titre { get; set; }

    [StringLength(60)]
    public string? Acteur { get; set; }

    [StringLength(60)]
    public string? Realisateur { get; set; }

    [StringLength(20)]
    public string? Genre { get; set; }

    [StringLength(30)]
    public string? Pays { get; set; }

    [StringLength(30)]
    public string? Langue { get; set; }

    [Range(1, 9999)]
    public int? AnneeMin { get; set; }

    [Range(1, 9999)]
    public int? AnneeMax { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnneeMin.HasValue && AnneeMax.HasValue && AnneeMin > AnneeMax)
            yield return new ValidationResult(
                "L'année minimale doit être inférieure ou égale à l'année maximale.",
                new[] { nameof(AnneeMin), nameof(AnneeMax) });
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("INTERPRETATION")]
[PrimaryKey(nameof(IdFilm), nameof(IdPersonne), nameof(NomPersonnage))]
public class Interpretation
{
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Column("IDPERSONNE")]
    public int IdPersonne { get; set; }

    [Required]
    [Column("NOMPERSONNAGE")]
    [MaxLength(80)]
    public string NomPersonnage { get; set; } = string.Empty;

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;

    [ForeignKey(nameof(IdPersonne))]
    public Personne Personne { get; set; } = null!;
}

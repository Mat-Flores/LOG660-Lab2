using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("FILMPAYS")]
[PrimaryKey(nameof(IdFilm), nameof(NomPays))]
public class FilmPays
{
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Required]
    [Column("NOMPAYS")]
    [MaxLength(30)]
    public string NomPays { get; set; } = string.Empty;

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;

    [ForeignKey(nameof(NomPays))]
    public Pays Pays { get; set; } = null!;
}

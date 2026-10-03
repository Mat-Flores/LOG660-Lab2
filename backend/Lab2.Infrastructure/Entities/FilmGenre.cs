using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("FILMGENRE")]
[PrimaryKey(nameof(IdFilm), nameof(NomGenre))]
public class FilmGenre
{
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Required]
    [Column("NOMGENRE")]
    [MaxLength(20)]
    public string NomGenre { get; set; } = string.Empty;

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;

    [ForeignKey(nameof(NomGenre))]
    public Genre Genre { get; set; } = null!;
}

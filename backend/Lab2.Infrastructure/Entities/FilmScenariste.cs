using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("FILMSCENARISTE")]
[PrimaryKey(nameof(IdFilm), nameof(IdPersonne))]
public class FilmScenariste
{
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Column("IDPERSONNE")]
    public int IdPersonne { get; set; }

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;

    [ForeignKey(nameof(IdPersonne))]
    public Personne Personne { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("BANDEANNONCE")]
[PrimaryKey(nameof(IdFilm), nameof(Lien))]
public class BandeAnnonce
{
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Required]
    [Column("LIEN")]
    [MaxLength(300)]
    public string Lien { get; set; } = string.Empty;

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;
}

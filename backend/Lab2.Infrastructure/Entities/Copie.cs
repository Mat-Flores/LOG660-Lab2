using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("COPIE")]
public class Copie
{
    [Key]
    [Column("CODECOPIE")]
    [MaxLength(20)]
    public string CodeCopie { get; set; } = string.Empty;

    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [ForeignKey(nameof(IdFilm))]
    public Film Film { get; set; } = null!;

    public ICollection<Location> Locations { get; set; } = new List<Location>();
}

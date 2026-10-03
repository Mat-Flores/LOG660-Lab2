using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("PAYS")]
public class Pays
{
    [Key]
    [Column("NOM")]
    [MaxLength(30)]
    public string Nom { get; set; } = string.Empty;

    public ICollection<FilmPays> FilmPays { get; set; } = new List<FilmPays>();
}

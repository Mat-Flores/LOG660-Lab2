using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("GENRE")]
public class Genre
{
    [Key]
    [Column("NOM")]
    [MaxLength(20)]
    public string Nom { get; set; } = string.Empty;

    public ICollection<FilmGenre> FilmGenres { get; set; } = new List<FilmGenre>();
}

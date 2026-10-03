using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("FILM")]
public class Film
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("IDFILM")]
    public int IdFilm { get; set; }

    [Column("IDREALISATEUR")]
    public int IdRealisateur { get; set; }

    [Required]
    [Column("TITRE")]
    [MaxLength(120)]
    public string Titre { get; set; } = string.Empty;

    [Column("ANNEESORTIE")]
    public int AnneeSortie { get; set; }

    [Column("DUREEMINUTES")]
    public int DureeMinutes { get; set; }

    [Required]
    [Column("LANGUEORIGINALE")]
    [MaxLength(30)]
    public string LangueOriginale { get; set; } = string.Empty;

    [Required]
    [Column("RESUME")]
    [MaxLength(1000)]
    public string Resume { get; set; } = string.Empty;

    [Required]
    [Column("URLAFFICHE")]
    [MaxLength(200)]
    public string UrlAffiche { get; set; } = string.Empty;

    [ForeignKey(nameof(IdRealisateur))]
    public Personne Realisateur { get; set; } = null!;

    public ICollection<BandeAnnonce> BandesAnnonces { get; set; } = new List<BandeAnnonce>();
    public ICollection<Interpretation> Interpretations { get; set; } = new List<Interpretation>();
    public ICollection<FilmScenariste> Scenaristes { get; set; } = new List<FilmScenariste>();
    public ICollection<FilmGenre> FilmGenres { get; set; } = new List<FilmGenre>();
    public ICollection<FilmPays> FilmPays { get; set; } = new List<FilmPays>();
    public ICollection<Copie> Copies { get; set; } = new List<Copie>();
}

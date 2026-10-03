using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("PERSONNE")]
public class Personne
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("IDPERSONNE")]
    public int IdPersonne { get; set; }

    [Required]
    [Column("NOM")]
    [MaxLength(60)]
    public string Nom { get; set; } = string.Empty;

    [Column("DATENAISSANCE")]
    public DateTime DateNaissance { get; set; }

    [Required]
    [Column("LIEUNAISSANCE")]
    [MaxLength(120)]
    public string LieuNaissance { get; set; } = string.Empty;

    [Required]
    [Column("PHOTO")]
    [MaxLength(200)]
    public string Photo { get; set; } = string.Empty;

    [Required]
    [Column("BIOGRAPHIE", TypeName = "CLOB")]
    public string Biographie { get; set; } = string.Empty;

    public ICollection<Film> FilmsRealises { get; set; } = new List<Film>();
    public ICollection<Interpretation> Interpretations { get; set; } = new List<Interpretation>();
    public ICollection<FilmScenariste> FilmsScenarises { get; set; } = new List<FilmScenariste>();
}

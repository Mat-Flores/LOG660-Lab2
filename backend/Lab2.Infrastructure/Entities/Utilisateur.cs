using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("UTILISATEUR")]
[Index(nameof(Courriel), IsUnique = true, Name = "UQ_UTILISATEUR_COURRIEL")]
public class Utilisateur
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // pas d'identité en BD : généré par p_ajouterClient
    [Column("IDUTILISATEUR")]
    public int IdUtilisateur { get; set; }

    [Required]
    [Column("NOMFAMILLE")]
    [MaxLength(40)]
    public string NomFamille { get; set; } = string.Empty;

    [Required]
    [Column("PRENOM")]
    [MaxLength(40)]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [Column("COURRIEL")]
    [MaxLength(80)]
    public string Courriel { get; set; } = string.Empty;

    [Required]
    [Column("TELEPHONE")]
    [MaxLength(20)]
    public string Telephone { get; set; } = string.Empty;

    [Column("DATENAISSANCE")]
    public DateTime DateNaissance { get; set; }

    [Required]
    [Column("MOTDEPASSE")]
    [MaxLength(50)]
    public string MotDePasse { get; set; } = string.Empty;

    public Adresse? Adresse { get; set; }
    public Client? Client { get; set; }
    public Employe? Employe { get; set; }
}

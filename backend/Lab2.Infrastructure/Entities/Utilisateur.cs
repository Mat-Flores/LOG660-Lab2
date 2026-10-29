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

    [Column("NOMFAMILLE")]
    [MaxLength(40)]
    public string? NomFamille { get; set; } = string.Empty;

    [Column("PRENOM")]
    [MaxLength(40)]
    public string? Prenom { get; set; } = string.Empty;

    [Column("COURRIEL")]
    [MaxLength(80)]
    public string? Courriel { get; set; } = string.Empty;

    [Column("TELEPHONE")]
    [MaxLength(20)]
    public string? Telephone { get; set; } = string.Empty;

    [Column("DATENAISSANCE")]
    public DateTime? DateNaissance { get; set; }

    [Column("MOTDEPASSE")]
    [MaxLength(50)]
    public string? MotDePasse { get; set; } = string.Empty;

    public Adresse? Adresse { get; set; }
    public Client? Client { get; set; }
    public Employe? Employe { get; set; }
}

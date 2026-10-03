using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("ADRESSE")]
public class Adresse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // PK = FK vers UTILISATEUR
    [Column("IDUTILISATEUR")]
    public int IdUtilisateur { get; set; }

    [Required]
    [Column("NUMEROCIVIQUE")]
    [MaxLength(10)]
    public string NumeroCivique { get; set; } = string.Empty;

    [Required]
    [Column("RUE")]
    [MaxLength(40)]
    public string Rue { get; set; } = string.Empty;

    [Required]
    [Column("VILLE")]
    [MaxLength(40)]
    public string Ville { get; set; } = string.Empty;

    [Required]
    [Column("PROVINCE")]
    [MaxLength(2)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [Column("CODEPOSTAL")]
    [MaxLength(7)]
    public string CodePostal { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUtilisateur))]
    public Utilisateur Utilisateur { get; set; } = null!;
}

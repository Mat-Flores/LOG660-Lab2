using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("EMPLOYE")]
[Index(nameof(Matricule), IsUnique = true, Name = "UQ_EMPLOYE_MATRICULE")]
public class Employe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // PK = FK vers UTILISATEUR
    [Column("IDUTILISATEUR")]
    public int IdUtilisateur { get; set; }

    [Required]
    [Column("MATRICULE")]
    [MaxLength(7)]
    public string Matricule { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUtilisateur))]
    public Utilisateur Utilisateur { get; set; } = null!;
}

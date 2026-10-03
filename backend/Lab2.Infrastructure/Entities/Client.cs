using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("CLIENT")]
public class Client
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // PK = FK vers UTILISATEUR
    [Column("IDUTILISATEUR")]
    public int IdUtilisateur { get; set; }

    [Required]
    [Column("CODEFORFAIT")]
    [MaxLength(1)]
    public string CodeForfait { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUtilisateur))]
    public Utilisateur Utilisateur { get; set; } = null!;

    [ForeignKey(nameof(CodeForfait))]
    public Forfait Forfait { get; set; } = null!;

    public CarteCredit? CarteCredit { get; set; }
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}

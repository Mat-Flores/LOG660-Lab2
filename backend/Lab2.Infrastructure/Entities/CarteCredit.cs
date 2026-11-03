using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("CARTECREDIT")]
public class CarteCredit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // PK = FK vers CLIENT
    [Column("IDUTILISATEUR")]
    public int IdUtilisateur { get; set; }

    [Column("TYPE")]
    [MaxLength(10)]
    public string? Type { get; set; } = string.Empty;

    [Column("NUMERO")]
    [MaxLength(19)]
    public string? Numero { get; set; } = string.Empty;

    [Column("DATEEXPIRATION")]
    public DateTime? DateExpiration { get; set; }

    [Column("CVV")]
    [MaxLength(4)]
    public string? Cvv { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUtilisateur))]
    public Client Client { get; set; } = null!;
}

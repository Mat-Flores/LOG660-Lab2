using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab2.Infrastructure.Entities;

[Table("CLIENT")]
public class Client : Utilisateur
{
    [Column("CODEFORFAIT")]
    [MaxLength(1)]
    public string? CodeForfait { get; set; } = string.Empty;

    [ForeignKey(nameof(CodeForfait))]
    public Forfait? Forfait { get; set; }

    public CarteCredit? CarteCredit { get; set; }
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}

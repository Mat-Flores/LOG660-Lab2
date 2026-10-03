using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("FORFAIT")]
public class Forfait
{
    [Key]
    [Column("CODE")]
    [MaxLength(1)]
    public string Code { get; set; } = string.Empty;

    [Column("COUT")]
    [Precision(5, 2)]
    public decimal Cout { get; set; }

    [Column("LOCATIONSMAX")]
    public int LocationsMax { get; set; }

    [Column("DUREEMAXJOURS")]
    public int? DureeMaxJours { get; set; }   // null = illimité (forfait A)

    public ICollection<Client> Clients { get; set; } = new List<Client>();
}

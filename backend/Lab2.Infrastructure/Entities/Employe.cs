using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Entities;

[Table("EMPLOYE")]
[Index(nameof(Matricule), IsUnique = true, Name = "UQ_EMPLOYE_MATRICULE")]
public class Employe : Utilisateur
{
    [Column("MATRICULE")]
    [MaxLength(7)]
    public string? Matricule { get; set; } = string.Empty;
}

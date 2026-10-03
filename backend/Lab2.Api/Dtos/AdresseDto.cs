using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

// Utilisé en sortie ET en entrée (création d'un client).
public class AdresseDto
{
    [Required, StringLength(10)]
    public string NumeroCivique { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Rue { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Ville { get; set; } = string.Empty;

    [Required, StringLength(2, MinimumLength = 2)]
    public string Province { get; set; } = string.Empty;

    [Required, StringLength(7)]
    public string CodePostal { get; set; } = string.Empty;
}

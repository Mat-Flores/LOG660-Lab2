using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

// Entrée du cas 2.
public class ConnexionDto
{
    [Required]
    public string Courriel { get; set; } = string.Empty;

    [Required]
    public string MotDePasse { get; set; } = string.Empty;
}

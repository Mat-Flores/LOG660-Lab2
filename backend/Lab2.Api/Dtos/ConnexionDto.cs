using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

public class ConnexionDto
{
    [Required, StringLength(80)]
    public string Courriel { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string MotDePasse { get; set; } = string.Empty;
}

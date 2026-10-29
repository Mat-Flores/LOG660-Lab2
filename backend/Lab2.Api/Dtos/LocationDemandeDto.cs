using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

public class LocationDemandeDto
{
    [Range(1, int.MaxValue)]
    public int IdFilm { get; set; }
}

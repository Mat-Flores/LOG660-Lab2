namespace Lab2.Api.Dtos;

public class LocationDto
{
    public int IdLocation { get; set; }
    public int IdFilm { get; set; }
    public string? Titre { get; set; }
    public string CodeCopie { get; set; } = string.Empty;
    public DateTime DateLocation { get; set; }
    public DateTime? DateRetourPrevue { get; set; }
}

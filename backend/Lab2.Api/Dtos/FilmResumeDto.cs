namespace Lab2.Api.Dtos;

public class FilmResumeDto
{
    public int IdFilm { get; set; }
    public string? Titre { get; set; } = string.Empty;
    public int? AnneeSortie { get; set; }
    public string? UrlAffiche { get; set; } = string.Empty;
}

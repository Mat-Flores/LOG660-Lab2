namespace Lab2.Api.Dtos;

public class FilmDto
{
    public int IdFilm { get; set; }
    public string? Titre { get; set; } = string.Empty;
    public int? AnneeSortie { get; set; }
    public int? DureeMinutes { get; set; }
    public string? LangueOriginale { get; set; } = string.Empty;
    public string? Resume { get; set; } = string.Empty;
    public string? UrlAffiche { get; set; } = string.Empty;
    public PersonneResumeDto? Realisateur { get; set; }
    public List<InterpretationDto> Acteurs { get; set; } = new();
    public List<PersonneResumeDto> Scenaristes { get; set; } = new();
    public List<string> Genres { get; set; } = new();
    public List<string> Pays { get; set; } = new();
    public List<string> BandesAnnonces { get; set; } = new();
}

namespace Lab2.Api.Dtos;

// Fiche descriptive du cas 3.
public class FilmDetailDto
{
    public int IdFilm { get; set; }
    public string Titre { get; set; } = string.Empty;
    public int AnneeSortie { get; set; }
    public List<string> Pays { get; set; } = new();
    public string? LangueOriginale { get; set; }
    public int DureeMinutes { get; set; }
    public List<string> Genres { get; set; } = new();
    public PersonneResumeDto? Realisateur { get; set; }
    public List<PersonneResumeDto> Scenaristes { get; set; } = new();
    public List<ActeurDto> Acteurs { get; set; } = new();
    public string Resume { get; set; } = string.Empty;
    public string? UrlAffiche { get; set; }
    public List<string> BandesAnnonces { get; set; } = new();

    // Permet au frontend de désactiver le bouton « Louer » (cas 4).
    public int CopiesDisponibles { get; set; }
}

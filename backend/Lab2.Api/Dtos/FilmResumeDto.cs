namespace Lab2.Api.Dtos;

// Élément de la liste de résultats du cas 3 : « Titanic (1997) ».
public class FilmResumeDto
{
    public int IdFilm { get; set; }
    public string Titre { get; set; } = string.Empty;
    public int AnneeSortie { get; set; }
    public string? UrlAffiche { get; set; }
}

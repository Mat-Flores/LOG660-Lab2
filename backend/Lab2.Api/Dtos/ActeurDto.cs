namespace Lab2.Api.Dtos;

// Un acteur peut jouer plusieurs personnages dans le même film.
public class ActeurDto
{
    public int IdPersonne { get; set; }
    public string? Nom { get; set; } = string.Empty;
    public List<string> Personnages { get; set; } = new();
}

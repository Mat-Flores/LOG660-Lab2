namespace Lab2.Api.Dtos;

// Valeurs proposées dans les listes déroulantes du formulaire de recherche.
public class CriteresRechercheDto
{
    public IReadOnlyList<string> Genres { get; set; } = [];
    public IReadOnlyList<string> Pays { get; set; } = [];
    public IReadOnlyList<string> Langues { get; set; } = [];
}

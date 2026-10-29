namespace Lab2.Api.Dtos;

public class UtilisateurDto
{
    public int IdUtilisateur { get; set; }
    public string? NomFamille { get; set; } = string.Empty;
    public string? Prenom { get; set; } = string.Empty;
    public string? Courriel { get; set; } = string.Empty;
}

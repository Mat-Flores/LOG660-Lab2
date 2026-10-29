namespace Lab2.Api.Dtos;

// Fiche d'un réalisateur ou d'un acteur (cas 3).
public class PersonneDto
{
    public int IdPersonne { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Photo { get; set; }
    public string? Biographie { get; set; }
}

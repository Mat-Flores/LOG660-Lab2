namespace Lab2.Api.Dtos;

public class PersonneDto
{
    public int IdPersonne { get; set; }
    public string? Nom { get; set; } = string.Empty;
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; } = string.Empty;
    public string? Photo { get; set; } = string.Empty;
    public string? Biographie { get; set; } = string.Empty;
}

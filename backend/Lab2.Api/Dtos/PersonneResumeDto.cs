namespace Lab2.Api.Dtos;

// Nom cliquable sur la fiche du film ; l'id mène à GET api/personnes/{id}.
public class PersonneResumeDto
{
    public int IdPersonne { get; set; }
    public string Nom { get; set; } = string.Empty;
}

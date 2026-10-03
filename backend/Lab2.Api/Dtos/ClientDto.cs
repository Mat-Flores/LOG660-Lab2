namespace Lab2.Api.Dtos;

// Aucun mot de passe en sortie.
public class ClientDto
{
    public int IdUtilisateur { get; set; }
    public string NomFamille { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Courriel { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }

    public AdresseDto? Adresse { get; set; }
    public ForfaitDto Forfait { get; set; } = null!;
    public CarteCreditDto? CarteCredit { get; set; }
}

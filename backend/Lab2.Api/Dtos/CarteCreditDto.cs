namespace Lab2.Api.Dtos;

// Ni le CVV ni le numéro complet ne sortent de l'API.
public class CarteCreditDto
{
    public string Type { get; set; } = string.Empty;
    public string NumeroMasque { get; set; } = string.Empty;
    public DateTime DateExpiration { get; set; }
}

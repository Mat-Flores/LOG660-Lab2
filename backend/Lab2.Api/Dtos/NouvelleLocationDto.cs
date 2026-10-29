using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

// Entrée du cas 4 : le client est celui de la session, la copie est choisie par le serveur.
public class NouvelleLocationDto
{
    [Range(1, int.MaxValue)]
    public int IdFilm { get; set; }
}

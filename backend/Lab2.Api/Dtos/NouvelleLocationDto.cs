using System.ComponentModel.DataAnnotations;

namespace Lab2.Api.Dtos;

// Entrée du cas 4 : idUtilisateur vient du ClientDto reçu au login ; la copie est choisie par le serveur.
public class NouvelleLocationDto
{
    [Range(1, int.MaxValue)]
    public int IdUtilisateur { get; set; }

    [Range(1, int.MaxValue)]
    public int IdFilm { get; set; }
}

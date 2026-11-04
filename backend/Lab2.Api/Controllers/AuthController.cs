using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

// Cas 2 : le frontend garde le ClientDto renvoyé (dont idUtilisateur) pour les appels suivants.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IClientService _service;

    public AuthController(IClientService service) => _service = service;

    // POST api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClientDto>> Login(ConnexionDto connexion, CancellationToken ct)
    {
        var client = await _service.AuthentifierAsync(connexion.Courriel, connexion.MotDePasse, ct);

        if (client is not null)
            return Ok(client);

        // Un compte employé valide est reconnu, mais l'application Webflix
        // réserve ce point d'entrée de connexion aux clients.
        var estEmploye = await _service.EmployeAuthentifieAsync(
            connexion.Courriel, connexion.MotDePasse, ct);

        if (estEmploye)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Accès réservé aux clients",
                detail: "Ce compte est un compte employé. Les employés ne peuvent pas se connecter à l'application Webflix; veuillez utiliser un compte client.");
        }

        return Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Connexion refusée",
            detail: "Courriel ou mot de passe invalide. Veuillez entrer de nouvelles valeurs.");
    }
}

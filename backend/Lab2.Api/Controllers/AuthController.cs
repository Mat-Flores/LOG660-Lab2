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
    public async Task<ActionResult<ClientDto>> Login(ConnexionDto connexion, CancellationToken ct)
    {
        var client = await _service.AuthentifierAsync(connexion.Courriel, connexion.MotDePasse, ct);

        // Même message dans les deux cas : on ne révèle pas si le courriel existe.
        return client is null
            ? Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Connexion refusée",
                detail: "Courriel ou mot de passe invalide. Veuillez entrer de nouvelles valeurs.")
            : Ok(client);
    }
}

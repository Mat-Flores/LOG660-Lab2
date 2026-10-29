using System.Security.Claims;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

// Cas 2 : la session est portée par un cookie HttpOnly posé à la connexion.
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
        if (client is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Connexion refusée",
                detail: "Courriel ou mot de passe invalide. Veuillez entrer de nouvelles valeurs.");
        }

        var identite = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, client.IdUtilisateur.ToString())],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identite));

        return Ok(client);
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }
}

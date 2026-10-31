using System.Globalization;
using System.Security.Claims;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lab2.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IClientService _service;
    private readonly IOptionsMonitor<BearerTokenOptions> _options;

    public AuthController(IClientService service, IOptionsMonitor<BearerTokenOptions> options)
    {
        _service = service;
        _options = options;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ClientDto>> Login(ConnexionDto connexion, CancellationToken ct)
    {
        var client = await _service.AuthentifierAsync(connexion.Courriel, connexion.MotDePasse, ct);
        if (client is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Connexion refusée",
                detail: "Courriel ou mot de passe invalide. Veuillez entrer de nouvelles valeurs.");

        var scheme = BearerTokenDefaults.AuthenticationScheme;
        var options = _options.Get(scheme);
        var identite = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, client.IdUtilisateur.ToString(CultureInfo.InvariantCulture))
        }, scheme);
        var proprietes = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.Add(options.BearerTokenExpiration)
        };

        // Le profil JSON reste identique ; le jeton est transmis séparément, sans cookie.
        Response.Headers["X-Auth-Token"] = options.BearerTokenProtector.Protect(
            new AuthenticationTicket(new ClaimsPrincipal(identite), proprietes, scheme));
        Response.Headers.CacheControl = "no-store";
        return Ok(client);
    }
}

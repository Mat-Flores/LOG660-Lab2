using System.Security.Claims;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthentificationController : ControllerBase
{
    private readonly IAuthentificationService _service;

    public AuthentificationController(IAuthentificationService service) => _service = service;

    [AllowAnonymous]
    [HttpPost("connexion")]
    public async Task<ActionResult<UtilisateurDto>> Connexion(ConnexionDto connexion, CancellationToken ct)
    {
        var utilisateur = await _service.ConnecterAsync(connexion, ct);

        if (utilisateur is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Courriel ou mot de passe incorrect."
            });
        }

        var identite = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, utilisateur.IdUtilisateur.ToString()) },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identite));

        return Ok(utilisateur);
    }

    [Authorize]
    [HttpPost("deconnexion")]
    public async Task<IActionResult> Deconnexion()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}

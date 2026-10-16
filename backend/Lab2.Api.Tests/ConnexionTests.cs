using System.Net;
using System.Net.Http.Json;
using Lab2.Api.Dtos;
using Lab2.Api.Services;
using Lab2.Infrastructure.Entities;
using Lab2.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lab2.Api.Tests;

public class ConnexionTests
{
    [Fact]
    public async Task Connexion_ConserveLaSession_EtDeconnexionLaTermine()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreerClient();

        var connexion = await client.PostAsJsonAsync("/api/auth/connexion", new ConnexionDto
        {
            Courriel = "  client@exemple.com  ",
            MotDePasse = "Secret123"
        });

        Assert.Equal(HttpStatusCode.OK, connexion.StatusCode);
        var utilisateur = await connexion.Content.ReadFromJsonAsync<UtilisateurDto>();
        Assert.Equal(42, utilisateur!.IdUtilisateur);
        var json = await connexion.Content.ReadAsStringAsync();
        Assert.DoesNotContain("motDePasse", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Secret123", json);
        var cookie = string.Join(";", connexion.Headers.GetValues("Set-Cookie"));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/clients/42")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/clients/99")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync("/api/auth/deconnexion", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/clients/42")).StatusCode);
    }

    [Theory]
    [InlineData("inconnu@exemple.com", "Secret123")]
    [InlineData("client@exemple.com", "incorrect")]
    [InlineData("client@exemple.com", "secret123")]
    [InlineData("client@exemple.com", " Secret123 ")]
    public async Task IdentifiantsInvalides_Retournent401_SansCreerDeSession(string courriel, string motDePasse)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreerClient();

        var reponse = await client.PostAsJsonAsync("/api/auth/connexion", new ConnexionDto
        {
            Courriel = courriel,
            MotDePasse = motDePasse
        });

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
        Assert.Contains("Courriel ou mot de passe incorrect.",
            (await reponse.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>())!.Title);
        Assert.False(reponse.Headers.Contains("Set-Cookie"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/clients/42")).StatusCode);
    }

    [Theory]
    [InlineData(null, "Secret123")]
    [InlineData("client@exemple.com", null)]
    [InlineData(" ", "Secret123")]
    [InlineData("client@exemple.com", " ")]
    public async Task ChampsManquants_Retournent400(string? courriel, string? motDePasse)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreerClient();

        var reponse = await client.PostAsJsonAsync("/api/auth/connexion", new { courriel, motDePasse });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        Assert.False(reponse.Headers.Contains("Set-Cookie"));
    }

    private class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                // Les tests utilisent des données fictives et ne se connectent jamais à Oracle.
                services.RemoveAll<IUtilisateurRepository>();
                services.AddScoped<IUtilisateurRepository, UtilisateurRepositoryFictif>();
                services.RemoveAll<IClientService>();
                services.AddScoped<IClientService, ClientServiceFictif>();
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
            });
        }

        public HttpClient CreerClient() => CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private class UtilisateurRepositoryFictif : IUtilisateurRepository
    {
        public Task<Utilisateur?> GetByCourrielAsync(string courriel, CancellationToken ct = default)
            => Task.FromResult(courriel == "client@exemple.com"
                ? new Utilisateur
                {
                    IdUtilisateur = 42,
                    NomFamille = "Test",
                    Prenom = "Client",
                    Courriel = courriel,
                    MotDePasse = "Secret123"
                }
                : null);
    }

    private class ClientServiceFictif : IClientService
    {
        public Task<ClientDto?> GetByIdAsync(int idUtilisateur, CancellationToken ct = default)
            => Task.FromResult<ClientDto?>(new ClientDto { IdUtilisateur = idUtilisateur });
    }
}

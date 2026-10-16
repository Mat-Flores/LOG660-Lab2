using System.Net.Http.Json;
using Lab2.Api.Dtos;
using Lab2.Infrastructure.Data;
using Lab2.Infrastructure.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Lab2.Api.Tests;

internal class CatalogueApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connexion = new("Data Source=:memory:");

    public CatalogueApiFactory() => _connexion.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            // Une base SQLite en mémoire remplace Oracle uniquement pendant les tests.
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connexion));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
        });
    }

    public async Task<HttpClient> CreerClientAsync(bool connecter = true)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        using var scope = Services.CreateScope();
        var contexte = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        contexte.Database.EnsureCreated();
        AjouterDonnees(contexte);

        if (connecter)
        {
            var reponse = await client.PostAsJsonAsync("/api/auth/connexion", new ConnexionDto
            {
                Courriel = "client@exemple.com",
                MotDePasse = "Secret123"
            });
            reponse.EnsureSuccessStatusCode();
        }

        return client;
    }

    private static void AjouterDonnees(AppDbContext contexte)
    {
        var lucas = Personne(1, "George Lucas");
        var hamill = Personne(2, "Mark Hamill");
        var ford = Personne(3, "Harrison Ford");
        var spielberg = Personne(4, "Steven Spielberg");
        var tom = Personne(5, "Tom Hardy");
        var hanks = Personne(6, "Jack Hanks");
        var jeunet = Personne(7, "Jean-Pierre Jeunet");
        var aventure = new Genre { Nom = "Aventure" };
        var scienceFiction = new Genre { Nom = "Science-fiction" };
        var comedie = new Genre { Nom = "Comedie" };
        var usa = new Pays { Nom = "USA" };
        var france = new Pays { Nom = "France" };

        var starWars = Film(10, "Star Wars", 1977, "English", lucas);
        starWars.FilmGenres.Add(new FilmGenre { Genre = aventure, NomGenre = aventure.Nom });
        starWars.FilmGenres.Add(new FilmGenre { Genre = scienceFiction, NomGenre = scienceFiction.Nom });
        starWars.FilmPays.Add(new FilmPays { Pays = usa, NomPays = usa.Nom });
        starWars.Interpretations.Add(new Interpretation { Personne = hamill, NomPersonnage = "Luke" });
        starWars.Interpretations.Add(new Interpretation { Personne = ford, NomPersonnage = "Han Solo" });
        starWars.Scenaristes.Add(new FilmScenariste { Personne = lucas });
        starWars.BandesAnnonces.Add(new BandeAnnonce { Lien = "https://exemple.com/bande-annonce" });

        var indiana = Film(11, "Indiana Jones", 1981, "English", spielberg);
        indiana.FilmGenres.Add(new FilmGenre { Genre = aventure, NomGenre = aventure.Nom });
        indiana.FilmPays.Add(new FilmPays { Pays = usa, NomPays = usa.Nom });
        indiana.Interpretations.Add(new Interpretation { Personne = ford, NomPersonnage = "Indiana" });
        // Ces deux noms distincts ne doivent pas correspondre à une recherche de « Tom Hanks ».
        indiana.Interpretations.Add(new Interpretation { Personne = tom, NomPersonnage = "Role A" });
        indiana.Interpretations.Add(new Interpretation { Personne = hanks, NomPersonnage = "Role B" });

        var amelie = Film(12, "Amelie", 2001, "French", jeunet);
        amelie.FilmGenres.Add(new FilmGenre { Genre = comedie, NomGenre = comedie.Nom });
        amelie.FilmPays.Add(new FilmPays { Pays = france, NomPays = france.Nom });

        contexte.Utilisateurs.Add(new Utilisateur
        {
            IdUtilisateur = 42,
            NomFamille = "Test",
            Prenom = "Client",
            Courriel = "client@exemple.com",
            MotDePasse = "Secret123",
            DateNaissance = new DateTime(1990, 1, 1)
        });
        contexte.Films.AddRange(starWars, indiana, amelie);
        contexte.SaveChanges();
    }

    private static Personne Personne(int id, string nom) => new()
    {
        IdPersonne = id,
        Nom = nom,
        DateNaissance = new DateTime(1970, 1, 1),
        LieuNaissance = "Lieu de test",
        Photo = "https://exemple.com/photo.jpg",
        Biographie = "Biographie de test"
    };

    private static Film Film(int id, string titre, int annee, string langue, Personne realisateur) => new()
    {
        IdFilm = id,
        Titre = titre,
        AnneeSortie = annee,
        DureeMinutes = 120,
        LangueOriginale = langue,
        Resume = "Resume de test",
        UrlAffiche = "https://exemple.com/affiche.jpg",
        Realisateur = realisateur
    };

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connexion.Dispose();
    }
}

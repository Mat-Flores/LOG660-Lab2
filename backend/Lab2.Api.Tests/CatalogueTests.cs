using System.Net;
using System.Net.Http.Json;
using Lab2.Api.Dtos;
using Xunit;

namespace Lab2.Api.Tests;

public class CatalogueTests
{
    [Theory]
    [InlineData("?titre=wArS%20sTa", new int[] { 10 })]
    [InlineData("?acteur=ha%20mar", new int[] { 10 })]
    [InlineData("?acteur=ford", new int[] { 10, 11 })]
    [InlineData("?realisateur=luc%20geo", new int[] { 10 })]
    [InlineData("?genre=SCIENCE-FICTION", new int[] { 10 })]
    [InlineData("?pays=france", new int[] { 12 })]
    [InlineData("?langue=FRENCH", new int[] { 12 })]
    [InlineData("?anneeMin=1977&anneeMax=1977", new int[] { 10 })]
    [InlineData("?anneeMin=1981", new int[] { 11, 12 })]
    [InlineData("?anneeMax=1981", new int[] { 10, 11 })]
    [InlineData("?acteur=ford&genre=aventure&pays=usa&langue=english&anneeMin=1980&anneeMax=1990", new int[] { 11 })]
    [InlineData("?acteur=tom%20hanks", new int[] { })]
    [InlineData("?titre=inexistant", new int[] { })]
    public async Task Recherche_AppliqueLesFiltres(string filtres, int[] idsAttendus)
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        var reponse = await client.GetAsync("/api/films" + filtres);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var films = await reponse.Content.ReadFromJsonAsync<List<FilmResumeDto>>();
        Assert.Equal(idsAttendus, films!.Select(f => f.IdFilm).OrderBy(id => id));
    }

    [Fact]
    public async Task SansFiltre_RetourneTousLesFilmsTriesParTitre()
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        var films = await client.GetFromJsonAsync<List<FilmResumeDto>>("/api/films");

        Assert.Equal(new[] { 12, 11, 10 }, films!.Select(f => f.IdFilm));
    }

    [Fact]
    public async Task FicheFilm_ContientLesInformationsEtLesPersonnesAssociees()
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        var film = await client.GetFromJsonAsync<FilmDto>("/api/films/10");

        Assert.Equal("Star Wars", film!.Titre);
        Assert.Equal(1977, film.AnneeSortie);
        Assert.Equal(120, film.DureeMinutes);
        Assert.Equal("English", film.LangueOriginale);
        Assert.Equal("Resume de test", film.Resume);
        Assert.Equal("https://exemple.com/affiche.jpg", film.UrlAffiche);
        Assert.Equal("George Lucas", film.Realisateur!.Nom);
        Assert.Equal(1, film.Realisateur.IdPersonne);
        Assert.Equal(new[] { "Aventure", "Science-fiction" }, film.Genres);
        Assert.Equal(new[] { "USA" }, film.Pays);
        Assert.Equal("George Lucas", Assert.Single(film.Scenaristes).Nom);
        Assert.Equal("https://exemple.com/bande-annonce", Assert.Single(film.BandesAnnonces));
        Assert.Contains(film.Acteurs, a => a.IdPersonne == 2 && a.Nom == "Mark Hamill" && a.NomPersonnage == "Luke");
        Assert.Contains(film.Acteurs, a => a.IdPersonne == 3 && a.NomPersonnage == "Han Solo");
    }

    [Fact]
    public async Task FichePersonne_ContientSaBiographie()
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        var personne = await client.GetFromJsonAsync<PersonneDto>("/api/personnes/2");

        Assert.Equal("Mark Hamill", personne!.Nom);
        Assert.Equal(new DateTime(1970, 1, 1), personne.DateNaissance);
        Assert.Equal("Lieu de test", personne.LieuNaissance);
        Assert.Equal("Biographie de test", personne.Biographie);
        Assert.Equal("https://exemple.com/photo.jpg", personne.Photo);
    }

    [Theory]
    [InlineData("/api/films/999")]
    [InlineData("/api/personnes/999")]
    public async Task IdentifiantInconnu_Retourne404(string url)
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);
    }

    [Theory]
    [InlineData("?anneeMin=2000&anneeMax=1900")]
    [InlineData("?anneeMin=0")]
    [InlineData("?anneeMax=10000")]
    [InlineData("?anneeMin=abc")]
    public async Task AnneesInvalides_Retournent400(string filtres)
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/films" + filtres)).StatusCode);
    }

    [Theory]
    [InlineData("/api/films")]
    [InlineData("/api/films/10")]
    [InlineData("/api/personnes/2")]
    public async Task SansConnexion_Retourne401(string url)
    {
        using var factory = new CatalogueApiFactory();
        using var client = await factory.CreerClientAsync(connecter: false);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(url)).StatusCode);
    }
}

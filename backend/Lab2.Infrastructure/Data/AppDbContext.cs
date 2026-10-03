using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Forfait> Forfaits => Set<Forfait>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Adresse> Adresses => Set<Adresse>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<CarteCredit> CartesCredit => Set<CarteCredit>();
    public DbSet<Employe> Employes => Set<Employe>();

    public DbSet<Personne> Personnes => Set<Personne>();
    public DbSet<Film> Films => Set<Film>();
    public DbSet<BandeAnnonce> BandesAnnonces => Set<BandeAnnonce>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Pays> ListePays => Set<Pays>();
    public DbSet<Interpretation> Interpretations => Set<Interpretation>();
    public DbSet<FilmScenariste> FilmScenaristes => Set<FilmScenariste>();
    public DbSet<FilmGenre> FilmGenres => Set<FilmGenre>();
    public DbSet<FilmPays> FilmsPays => Set<FilmPays>();

    public DbSet<Copie> Copies => Set<Copie>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Les colonnes sont des VARCHAR2 et DATE (pas NVARCHAR2 / TIMESTAMP) :
        // évite les conversions implicites qui empêchent l'usage des index.
        configurationBuilder.Properties<string>().AreUnicode(false);
        configurationBuilder.Properties<DateTime>().HaveColumnType("DATE");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Les scripts SQL créent des identifiants non quotés, donc Oracle les stocke en MAJUSCULES
        // (CARTECREDIT, IDUTILISATEUR, ...). EF les met entre guillemets : on force les majuscules.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            var builder = modelBuilder.Entity(entityType.ClrType);
            builder.ToTable(entityType.ClrType.Name.ToUpperInvariant());

            foreach (var property in entityType.GetProperties().ToList())
                builder.Property(property.Name).HasColumnName(property.Name.ToUpperInvariant());
        }
    }
}

using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class FilmScenaristeConfiguration : IEntityTypeConfiguration<FilmScenariste>
{
    public void Configure(EntityTypeBuilder<FilmScenariste> b)
    {
        b.HasKey(x => new { x.IdFilm, x.IdPersonne });

        b.HasOne(x => x.Film)
            .WithMany(f => f.Scenaristes)
            .HasForeignKey(x => x.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Personne)
            .WithMany(p => p.FilmsScenarises)
            .HasForeignKey(x => x.IdPersonne)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

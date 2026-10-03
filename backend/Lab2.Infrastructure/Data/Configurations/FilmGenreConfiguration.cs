using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class FilmGenreConfiguration : IEntityTypeConfiguration<FilmGenre>
{
    public void Configure(EntityTypeBuilder<FilmGenre> b)
    {
        b.HasKey(x => new { x.IdFilm, x.NomGenre });

        b.HasOne(x => x.Film)
            .WithMany(f => f.FilmGenres)
            .HasForeignKey(x => x.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Genre)
            .WithMany(g => g.FilmGenres)
            .HasForeignKey(x => x.NomGenre)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

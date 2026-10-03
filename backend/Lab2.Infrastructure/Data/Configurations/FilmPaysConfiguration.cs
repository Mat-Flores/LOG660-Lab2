using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class FilmPaysConfiguration : IEntityTypeConfiguration<FilmPays>
{
    public void Configure(EntityTypeBuilder<FilmPays> b)
    {
        b.HasKey(x => new { x.IdFilm, x.NomPays });

        b.HasOne(x => x.Film)
            .WithMany(f => f.FilmPays)
            .HasForeignKey(x => x.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Pays)
            .WithMany(p => p.FilmPays)
            .HasForeignKey(x => x.NomPays)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

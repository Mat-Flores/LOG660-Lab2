using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class FilmConfiguration : IEntityTypeConfiguration<Film>
{
    public void Configure(EntityTypeBuilder<Film> b)
    {
        b.HasKey(f => f.IdFilm);
        b.Property(f => f.IdFilm).ValueGeneratedNever();

        b.HasOne(f => f.Realisateur)
            .WithMany(p => p.FilmsRealises)
            .HasForeignKey(f => f.IdRealisateur)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class InterpretationConfiguration : IEntityTypeConfiguration<Interpretation>
{
    public void Configure(EntityTypeBuilder<Interpretation> b)
    {
        b.HasKey(i => new { i.IdFilm, i.IdPersonne, i.NomPersonnage });

        b.HasOne(i => i.Film)
            .WithMany(f => f.Interpretations)
            .HasForeignKey(i => i.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(i => i.Personne)
            .WithMany(p => p.Interpretations)
            .HasForeignKey(i => i.IdPersonne)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

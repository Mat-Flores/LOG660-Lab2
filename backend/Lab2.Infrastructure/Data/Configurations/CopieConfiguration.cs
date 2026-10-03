using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class CopieConfiguration : IEntityTypeConfiguration<Copie>
{
    public void Configure(EntityTypeBuilder<Copie> b)
    {
        b.HasKey(c => c.CodeCopie);

        b.HasOne(c => c.Film)
            .WithMany(f => f.Copies)
            .HasForeignKey(c => c.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

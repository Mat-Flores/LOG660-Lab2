using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class BandeAnnonceConfiguration : IEntityTypeConfiguration<BandeAnnonce>
{
    public void Configure(EntityTypeBuilder<BandeAnnonce> b)
    {
        b.HasKey(x => new { x.IdFilm, x.Lien });

        b.HasOne(x => x.Film)
            .WithMany(f => f.BandesAnnonces)
            .HasForeignKey(x => x.IdFilm)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

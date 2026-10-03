using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class AdresseConfiguration : IEntityTypeConfiguration<Adresse>
{
    public void Configure(EntityTypeBuilder<Adresse> b)
    {
        b.HasKey(a => a.IdUtilisateur);

        b.HasOne(a => a.Utilisateur)
            .WithOne(u => u.Adresse)
            .HasForeignKey<Adresse>(a => a.IdUtilisateur)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

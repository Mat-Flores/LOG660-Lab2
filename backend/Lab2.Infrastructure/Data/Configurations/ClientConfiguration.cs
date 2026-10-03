using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.HasKey(c => c.IdUtilisateur);

        b.HasOne(c => c.Utilisateur)
            .WithOne(u => u.Client)
            .HasForeignKey<Client>(c => c.IdUtilisateur)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(c => c.Forfait)
            .WithMany(f => f.Clients)
            .HasForeignKey(c => c.CodeForfait)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

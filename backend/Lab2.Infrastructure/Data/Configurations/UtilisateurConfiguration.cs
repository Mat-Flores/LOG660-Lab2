using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class UtilisateurConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> b)
    {
        b.HasKey(u => u.IdUtilisateur);
        b.Property(u => u.IdUtilisateur).ValueGeneratedNever();   // pas d'identité en BD : généré par p_ajouterClient
    }
}

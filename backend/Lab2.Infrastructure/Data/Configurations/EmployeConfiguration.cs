using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class EmployeConfiguration : IEntityTypeConfiguration<Employe>
{
    public void Configure(EntityTypeBuilder<Employe> b)
    {
        b.HasKey(e => e.IdUtilisateur);

        b.HasOne(e => e.Utilisateur)
            .WithOne(u => u.Employe)
            .HasForeignKey<Employe>(e => e.IdUtilisateur)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

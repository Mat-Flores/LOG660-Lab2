using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class CarteCreditConfiguration : IEntityTypeConfiguration<CarteCredit>
{
    public void Configure(EntityTypeBuilder<CarteCredit> b)
    {
        b.HasKey(c => c.IdUtilisateur);

        b.HasOne(c => c.Client)
            .WithOne(cl => cl.CarteCredit)
            .HasForeignKey<CarteCredit>(c => c.IdUtilisateur)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

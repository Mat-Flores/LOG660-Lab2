using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class ForfaitConfiguration : IEntityTypeConfiguration<Forfait>
{
    public void Configure(EntityTypeBuilder<Forfait> b)
    {
        b.HasKey(f => f.Code);
        b.Property(f => f.Cout).HasPrecision(5, 2);
    }
}

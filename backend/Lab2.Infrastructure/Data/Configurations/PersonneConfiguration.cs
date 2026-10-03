using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class PersonneConfiguration : IEntityTypeConfiguration<Personne>
{
    public void Configure(EntityTypeBuilder<Personne> b)
    {
        b.HasKey(p => p.IdPersonne);
        b.Property(p => p.IdPersonne).ValueGeneratedNever();
        b.Property(p => p.Biographie).HasColumnType("CLOB");
    }
}

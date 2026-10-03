using Lab2.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lab2.Infrastructure.Data.Configurations;

public class PaysConfiguration : IEntityTypeConfiguration<Pays>
{
    public void Configure(EntityTypeBuilder<Pays> b) => b.HasKey(p => p.Nom);
}

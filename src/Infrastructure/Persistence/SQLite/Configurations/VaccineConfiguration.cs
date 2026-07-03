using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.SQLite.Configurations;

public class VaccineConfiguration : IEntityTypeConfiguration<Vaccine>
{
    public void Configure(EntityTypeBuilder<Vaccine> builder)
    {
        builder.ToTable("Vaccines");

        builder.HasKey(vaccine => vaccine.Id);

        builder.Property(vaccine => vaccine.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(vaccine => vaccine.Name)
            .IsUnique(); 

        // Coluna anulável: null = vacina periódica (sem limite de doses).
        builder.Property(vaccine => vaccine.TotalDoses);
        
        builder.Property<bool>("IsDeleted")
            .HasDefaultValue(false);

        builder.HasQueryFilter(vaccine => EF.Property<bool>(vaccine, "IsDeleted") == false);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.SQLite.Configurations;

public class VaccinationConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("Vaccinations");

        builder.HasKey(vaccination => vaccination.Id);

        builder.Property(vaccination => vaccination.Dose)
            .IsRequired();

        builder.Property(vaccination => vaccination.ApplicationDate)
            .IsRequired();

        builder.HasOne<Patient>()
            .WithMany(patient => patient.Vaccinations)
            .HasForeignKey(vaccination => vaccination.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Vaccine>()
            .WithMany()
            .HasForeignKey(vaccination => vaccination.VaccineId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasIndex(vaccination => new { vaccination.PatientId, vaccination.VaccineId, vaccination.Dose })
            .IsUnique();
        
        builder.Property<bool>("IsDeleted")
            .HasDefaultValue(false);

        builder.HasQueryFilter(vaccination => EF.Property<bool>(vaccination, "IsDeleted") == false);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.SQLite.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property<bool>("IsDeleted")
            .HasDefaultValue(false);

        builder.HasQueryFilter(patient => EF.Property<bool>(patient, "IsDeleted") == false);
        
        builder.Metadata
            .FindNavigation(nameof(Patient.Vaccinations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLite.Client;

public class SqliteContext : DbContext
{
    public SqliteContext(DbContextOptions<SqliteContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
    public DbSet<Vaccine> Vaccines => Set<Vaccine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqliteContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
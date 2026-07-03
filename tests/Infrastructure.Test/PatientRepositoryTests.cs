using Domain.Entities;
using Infrastructure.Persistence.SQLite.Client;
using Infrastructure.Persistence.SQLite.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTest;

public class PatientRepositoryTests : IDisposable
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteContext> _options;

    public PatientRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SqliteContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new SqliteContext(_options);
        context.Database.EnsureCreated();
    }

    private SqliteContext NewContext() => new(_options);

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task GetByIdAsync_IncludesVaccinations()
    {
        var patientId = await SeedPatientWithDoses(1, 2);

        await using var context = NewContext();
        var repository = new PatientRepository(context);

        var patient = await repository.GetByIdAsync(patientId);

        Assert.NotNull(patient);
        Assert.Equal(2, patient!.Vaccinations.Count);
    }

    [Fact]
    public async Task SoftDeleteAsync_HidesPatient()
    {
        var patientId = await SeedPatientWithDoses(1);

        await using (var context = NewContext())
        {
            await new PatientRepository(context).SoftDeleteAsync(patientId);
        }

        await using (var context = NewContext())
        {
            var patient = await new PatientRepository(context).GetByIdAsync(patientId);
            Assert.Null(patient);
        }
    }

    [Fact]
    public async Task SoftDeleteAsync_CascadesToVaccinations()
    {
        var patientId = await SeedPatientWithDoses(1, 2);

        await using (var context = NewContext())
        {
            await new PatientRepository(context).SoftDeleteAsync(patientId);
        }

        await using (var context = NewContext())
        {
            Assert.Equal(0, await context.Vaccinations.CountAsync());
            Assert.Equal(2, await context.Vaccinations.IgnoreQueryFilters().CountAsync());
        }
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsMatchingPatient()
    {
        await SeedPatientWithDoses();

        await using var context = NewContext();
        var repository = new PatientRepository(context);

        var patient = await repository.GetByUsernameAsync("kauan");

        Assert.NotNull(patient);
        Assert.Equal("kauan", patient!.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenMissing_ReturnsNull()
    {
        await using var context = NewContext();
        var repository = new PatientRepository(context);

        var patient = await repository.GetByUsernameAsync("ghost");

        Assert.Null(patient);
    }

    private async Task<Guid> SeedPatientWithDoses(params int[] doses)
    {
        await using var context = NewContext();
        var repository = new PatientRepository(context);

        var vaccine = new Vaccine("COVID-19", totalDoses: 3);
        context.Vaccines.Add(vaccine);

        var patient = new Patient("Kauan", "kauan", "hash");
        foreach (var dose in doses)
            patient.AddVaccination(vaccine, dose, Today);

        await repository.CreateAsync(patient);
        return patient.Id;
    }
}

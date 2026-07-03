using Domain.Entities;
using Infrastructure.Persistence.SQLite.Client;
using Infrastructure.Persistence.SQLite.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTest;

public class VaccineRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteContext> _options;

    public VaccineRepositoryTests()
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
    public async Task ListAsync_ReturnsFirstPageOrderedByName_ExcludingDeleted()
    {
        await using (var context = NewContext())
        {
            var repository = new VaccineRepository(context);
            await repository.CreateAsync(new Vaccine("COVID-19", 3));
            await repository.CreateAsync(new Vaccine("BCG", 1));
            await repository.CreateAsync(new Vaccine("Hepatite B", 3));

            var deleted = new Vaccine("Febre Amarela", 1);
            await repository.CreateAsync(deleted);
            await repository.SoftDeleteAsync(deleted.Id);
        }

        await using (var context = NewContext())
        {
            var repository = new VaccineRepository(context);
            var (items, totalCount) = await repository.ListAsync(page: 1, pageSize: 2);

            Assert.Equal(3, totalCount);
            Assert.Equal(2, items.Count);
            Assert.Equal("BCG", items[0].Name);
            Assert.Equal("COVID-19", items[1].Name);
        }
    }

    [Fact]
    public async Task ListAsync_SecondPage_ReturnsRemainder()
    {
        await using (var context = NewContext())
        {
            var repository = new VaccineRepository(context);
            await repository.CreateAsync(new Vaccine("BCG", 1));
            await repository.CreateAsync(new Vaccine("COVID-19", 3));
            await repository.CreateAsync(new Vaccine("Hepatite B", 3));
        }

        await using (var context = NewContext())
        {
            var repository = new VaccineRepository(context);
            var (items, totalCount) = await repository.ListAsync(page: 2, pageSize: 2);

            Assert.Equal(3, totalCount);
            Assert.Single(items);
            Assert.Equal("Hepatite B", items[0].Name);
        }
    }
}

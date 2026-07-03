using Domain.Entities;
using Infrastructure.Persistence.SQLite.Client;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLite.Seed;

public static class VaccineSeeder
{
    public static async Task SeedAsync(SqliteContext context)
    {
        if (await context.Vaccines.IgnoreQueryFilters().AnyAsync())
            return;

        var vaccines = new[]
        {
            new Vaccine("COVID-19", 2),
            new Vaccine("Febre Amarela", 1),
            new Vaccine("Hepatite B", 3),
            new Vaccine("Tríplice Viral (Sarampo, Caxumba e Rubéola)", 2),
            new Vaccine("Influenza (Gripe)", totalDoses: null),
            new Vaccine("dT (Difteria e Tétano)", totalDoses: null),
        };

        await context.Vaccines.AddRangeAsync(vaccines);
        await context.SaveChangesAsync();
    }
}

using Application.Ports.Persistence.Repositories;
using Infrastructure.Persistence.SQLite.Client;
using Infrastructure.Persistence.SQLite.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Api.Bootstrap;

public class DatabaseDiConfiguration
{
    public static IServiceCollection AddDependencyInjectionConfig(IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SqliteContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IVaccineRepository, VaccineRepository>();
        services.AddScoped<IVaccinationRepository, VaccinationRepository>();

        return services;
    }
}
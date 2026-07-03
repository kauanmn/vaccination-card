using Application.UseCases.Patients;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Bootstrap;

public static class ApplicationDiConfiguration
{
    public static IServiceCollection AddApplicationDependencyInjectionConfiguration(IServiceCollection services)
    {
        AddAllUseCasesFromAssembly(services, typeof(GetPatientById));
        return services;
    }

    public static void AddAllUseCasesFromAssembly(this IServiceCollection services, Type typeInAssembly)
    {
        var useCaseTypes = typeInAssembly.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.Namespace is not null
                        && t.Namespace.Contains(".UseCases"));

        foreach (var useCaseType in useCaseTypes)
        {
            services.TryAddScoped(useCaseType);
        }
    }
}
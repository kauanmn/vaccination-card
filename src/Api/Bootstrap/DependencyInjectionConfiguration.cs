namespace Api.Bootstrap;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddDependencyInjectionConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        ApplicationDiConfiguration.AddApplicationDependencyInjectionConfiguration(services);
        DatabaseDiConfiguration.AddDependencyInjectionConfig(services, configuration);
        services.AddAuthDependencyInjectionConfiguration(configuration);
        services.AddCorsDependencyInjectionConfiguration(configuration);
        return services;
    }
}
namespace Api.Bootstrap;

public static class CorsDiConfiguration
{
    public const string PolicyName = "Frontend";

    public static IServiceCollection AddCorsDependencyInjectionConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(PolicyName, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        return services;
    }
}

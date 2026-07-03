using System.Security.Claims;
using System.Text;
using Api.Http;
using Application.Ports.Security;
using Application.Security;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Bootstrap;

public static class AuthDiConfiguration
{
    public const string AdminOnlyPolicy = "AdminOnly";

    public static IServiceCollection AddAuthDependencyInjectionConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        var jwt = authOptions.Jwt;

        services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await WriteError(context.Response, StatusCodes.Status401Unauthorized,
                            "UNAUTHORIZED", "Autenticação necessária.");
                    },
                    OnForbidden = context =>
                        WriteError(context.Response, StatusCodes.Status403Forbidden,
                            "FORBIDDEN", "Acesso negado.")
                };
            });

        services.AddAuthorization(options =>
            options.AddPolicy(AdminOnlyPolicy, policy => policy.RequireRole(Roles.Admin)));

        return services;
    }

    private static async Task WriteError(HttpResponse response, int status, string code, string message)
    {
        if (response.HasStarted)
            return;

        response.Clear();
        response.StatusCode = status;

        var payload = new ErrorResponse(false, new ApiError(code, message));
        await response.WriteAsJsonAsync(payload, ApiJson.Options);
    }
}

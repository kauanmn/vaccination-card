using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiTest;

public class VaccinationApiFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "admin123";

    private const string AdminPasswordHash =
        "AQAAAAIAAYagAAAAENnqsozQ+2rOE6JAwex4YKk9hdHRKUrPt4Qb0FDQkcjT7VS/rulEF3oUZ1wJAc9vBA==";

    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"vaccard-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection",
            $"Data Source={_databasePath};Pooling=False");

        builder.UseSetting("Auth:Jwt:Key", "integration-tests-signing-key-at-least-32-bytes!");
        builder.UseSetting("Auth:Jwt:Issuer", "vaccination-card-api");
        builder.UseSetting("Auth:Jwt:Audience", "vaccination-card-client");
        builder.UseSetting("Auth:Jwt:ExpiryMinutes", "60");
        builder.UseSetting("Auth:Admin:Username", AdminUsername);
        builder.UseSetting("Auth:Admin:PasswordHash", AdminPasswordHash);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        try
        {
            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
        }
        catch (IOException)
        {
        }
    }
}

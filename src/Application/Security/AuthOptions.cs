namespace Application.Security;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public JwtOptions Jwt { get; set; } = new();
    public AdminOptions Admin { get; set; } = new();
}

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

public class AdminOptions
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

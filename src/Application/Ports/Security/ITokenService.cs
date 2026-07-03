namespace Application.Ports.Security;

public interface ITokenService
{
    AccessToken Generate(Guid subject, string role, string name, string username);
}

public record AccessToken(string Token, DateTime ExpiresAtUtc);

using Application.Ports.Security;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security;

public class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly object User = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(User, password);

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(User, hash, password);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

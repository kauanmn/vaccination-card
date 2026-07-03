using Infrastructure.Security;
using Xunit;

namespace InfrastructureTest;

public class IdentityPasswordHasherTests
{
    private readonly IdentityPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesVerifiableNonPlaintextHash()
    {
        var hash = _hasher.Hash("s3cret");

        Assert.NotEqual("s3cret", hash);
        Assert.True(_hasher.Verify("s3cret", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("s3cret");

        Assert.False(_hasher.Verify("wrong", hash));
    }
}

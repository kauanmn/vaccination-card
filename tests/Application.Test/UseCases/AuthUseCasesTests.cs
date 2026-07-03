using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Ports.Persistence.Repositories;
using Application.Ports.Security;
using Application.Security;
using Application.UseCases.Auth;
using Domain.Entities;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ApplicationTest.UseCases;

public class AuthUseCasesTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private readonly AccessToken _issued = new("token-value", DateTime.UtcNow.AddHours(1));

    private Login BuildLogin()
    {
        _tokenService.Generate(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_issued);

        var options = Options.Create(new AuthOptions
        {
            Admin = new AdminOptions { Username = "admin", PasswordHash = "admin-hash" }
        });

        return new Login(_patientRepository, _passwordHasher, _tokenService, options);
    }

    [Fact]
    public async Task Login_WithAdminCredentials_ReturnsAdminToken()
    {
        _passwordHasher.Verify("admin123", "admin-hash").Returns(true);

        var response = await BuildLogin().RunAsync(new LoginRequest { Username = "admin", Password = "admin123" });

        Assert.Equal(Roles.Admin, response.Role);
        Assert.Equal("token-value", response.Token);
        _tokenService.Received(1).Generate(Guid.Empty, Roles.Admin, "admin", "admin");
        await _patientRepository.DidNotReceive().GetByUsernameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Login_WithValidPatientCredentials_ReturnsPatientToken()
    {
        var patient = new Patient("Kauan", "kauan", "stored-hash");
        _patientRepository.GetByUsernameAsync("kauan").Returns(patient);
        _passwordHasher.Verify("secret", "stored-hash").Returns(true);

        var response = await BuildLogin().RunAsync(new LoginRequest { Username = "kauan", Password = "secret" });

        Assert.Equal(Roles.Patient, response.Role);
        Assert.Equal("token-value", response.Token);
        _tokenService.Received(1).Generate(patient.Id, Roles.Patient, "Kauan", "kauan");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var patient = new Patient("Kauan", "kauan", "stored-hash");
        _patientRepository.GetByUsernameAsync("kauan").Returns(patient);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => BuildLogin().RunAsync(new LoginRequest { Username = "kauan", Password = "wrong" }));
    }

    [Fact]
    public async Task Login_WithUnknownUser_ThrowsInvalidCredentials()
    {
        _patientRepository.GetByUsernameAsync(Arg.Any<string>()).Returns((Patient?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => BuildLogin().RunAsync(new LoginRequest { Username = "ghost", Password = "secret" }));
    }

    [Fact]
    public async Task Login_WithAdminUsernameButWrongPassword_FallsThroughToInvalidCredentials()
    {
        _patientRepository.GetByUsernameAsync("admin").Returns((Patient?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => BuildLogin().RunAsync(new LoginRequest { Username = "admin", Password = "nope" }));
    }
}

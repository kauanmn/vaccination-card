using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Ports.Persistence.Repositories;
using Application.Ports.Security;
using Application.Security;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Auth;

public class Login
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly AuthOptions _authOptions;

    public Login(
        IPatientRepository patientRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<AuthOptions> authOptions)
    {
        _patientRepository = patientRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _authOptions = authOptions.Value;
    }

    public async Task<LoginResponse> RunAsync(LoginRequest request)
    {
        var admin = _authOptions.Admin;
        if (!string.IsNullOrEmpty(admin.Username)
            && !string.IsNullOrEmpty(admin.PasswordHash)
            && string.Equals(request.Username, admin.Username, StringComparison.Ordinal)
            && _passwordHasher.Verify(request.Password, admin.PasswordHash))
        {
            var adminToken = _tokenService.Generate(Guid.Empty, Roles.Admin, admin.Username, admin.Username);
            return ToResponse(adminToken, Roles.Admin);
        }

        var patient = await _patientRepository.GetByUsernameAsync(request.Username);
        if (patient is null || !_passwordHasher.Verify(request.Password, patient.PasswordHash))
            throw new InvalidCredentialsException();

        var token = _tokenService.Generate(patient.Id, Roles.Patient, patient.Name, patient.Username);
        return ToResponse(token, Roles.Patient);
    }

    private static LoginResponse ToResponse(AccessToken token, string role) => new()
    {
        Token = token.Token,
        Role = role,
        ExpiresAt = token.ExpiresAtUtc
    };
}

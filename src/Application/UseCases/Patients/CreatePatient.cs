using System.Globalization;
using System.Text;
using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Application.Ports.Security;
using Application.Security;
using Domain.Entities;

namespace Application.UseCases.Patients;

public class CreatePatient
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreatePatient(IPatientRepository patientRepository, IPasswordHasher passwordHasher)
    {
        _patientRepository = patientRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreatePatientResponse> RunAsync(CreatePatientRequest request)
    {
        var username = await GenerateUniqueUsernameAsync(request.Name);
        var password = PasswordGenerator.Generate();
        var passwordHash = _passwordHasher.Hash(password);

        var patient = new Patient(request.Name, username, passwordHash);

        await _patientRepository.CreateAsync(patient);

        return new CreatePatientResponse
        {
            Id = patient.Id,
            Name = patient.Name,
            Username = patient.Username,
            Password = password
        };
    }

    private async Task<string> GenerateUniqueUsernameAsync(string name)
    {
        var baseUsername = Slugify(name);
        if (baseUsername.Length == 0)
            baseUsername = "paciente";

        var candidate = baseUsername;
        var suffix = 1;

        while (await _patientRepository.GetByUsernameAsync(candidate) is not null)
            candidate = $"{baseUsername}{++suffix}";

        return candidate;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
                builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }
}

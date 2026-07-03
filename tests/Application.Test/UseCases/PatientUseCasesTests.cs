using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Application.Ports.Security;
using Application.UseCases.Patients;
using Domain.Entities;
using Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace ApplicationTest.UseCases;

public class PatientUseCasesTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    private CreatePatient BuildCreate()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed");
        return new CreatePatient(_patientRepository, _passwordHasher);
    }

    [Fact]
    public async Task CreatePatient_GeneratesCredentialsAndPersists()
    {
        var response = await BuildCreate().RunAsync(new CreatePatientRequest { Name = "Kauan" });

        Assert.Equal("Kauan", response.Name);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.False(string.IsNullOrWhiteSpace(response.Username));
        Assert.False(string.IsNullOrWhiteSpace(response.Password));

        await _patientRepository.Received(1).CreateAsync(
            Arg.Is<Patient>(p => p.Name == "Kauan" && p.PasswordHash == "hashed"));
        _passwordHasher.Received(1).Hash(response.Password);
    }

    [Fact]
    public async Task CreatePatient_DerivesUsernameFromName()
    {
        var response = await BuildCreate().RunAsync(new CreatePatientRequest { Name = "José da Silva" });

        Assert.Equal("josedasilva", response.Username);
    }

    [Fact]
    public async Task CreatePatient_WhenUsernameTaken_AppendsSuffix()
    {
        _patientRepository.GetByUsernameAsync("kauan")
            .Returns(new Patient("Kauan", "kauan", "hash"));

        var response = await BuildCreate().RunAsync(new CreatePatientRequest { Name = "Kauan" });

        Assert.Equal("kauan2", response.Username);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidName_ThrowsAndDoesNotPersist()
    {
        await Assert.ThrowsAsync<InvalidPatientException>(
            () => BuildCreate().RunAsync(new CreatePatientRequest { Name = "" }));

        await _patientRepository.DidNotReceive().CreateAsync(Arg.Any<Patient>());
    }

    [Fact]
    public async Task GetPatientById_WhenExists_ReturnsResponse()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new GetPatientById(_patientRepository);

        var response = await useCase.RunAsync(patient.Id);

        Assert.Equal(patient.Id, response.Id);
        Assert.Equal("Kauan", response.Name);
    }

    [Fact]
    public async Task GetPatientById_WhenMissing_ThrowsNotFound()
    {
        _patientRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Patient?)null);
        var useCase = new GetPatientById(_patientRepository);

        await Assert.ThrowsAsync<PatientNotFound>(() => useCase.RunAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdatePatient_WhenExists_RenamesAndPersists()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new UpdatePatient(_patientRepository);

        var response = await useCase.RunAsync(patient.Id,
            new UpdatePatientRequest { Name = "Kauan Manzato" });

        Assert.Equal("Kauan Manzato", response.Name);
        await _patientRepository.Received(1).UpdateAsync(
            Arg.Is<Patient>(p => p.Id == patient.Id && p.Name == "Kauan Manzato"));
    }

    [Fact]
    public async Task UpdatePatient_WhenMissing_ThrowsAndDoesNotPersist()
    {
        _patientRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Patient?)null);
        var useCase = new UpdatePatient(_patientRepository);

        await Assert.ThrowsAsync<PatientNotFound>(
            () => useCase.RunAsync(Guid.NewGuid(), new UpdatePatientRequest { Name = "Novo" }));

        await _patientRepository.DidNotReceive().UpdateAsync(Arg.Any<Patient>());
    }

    [Fact]
    public async Task UpdatePatient_WithInvalidName_ThrowsAndDoesNotPersist()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new UpdatePatient(_patientRepository);

        await Assert.ThrowsAsync<InvalidPatientException>(
            () => useCase.RunAsync(patient.Id, new UpdatePatientRequest { Name = "" }));

        await _patientRepository.DidNotReceive().UpdateAsync(Arg.Any<Patient>());
    }

    [Fact]
    public async Task DeletePatient_WhenExists_SoftDeletes()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new DeletePatient(_patientRepository);

        await useCase.RunAsync(patient.Id);

        await _patientRepository.Received(1).SoftDeleteAsync(patient.Id);
    }

    [Fact]
    public async Task DeletePatient_WhenMissing_ThrowsAndDoesNotDelete()
    {
        _patientRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Patient?)null);
        var useCase = new DeletePatient(_patientRepository);

        await Assert.ThrowsAsync<PatientNotFound>(() => useCase.RunAsync(Guid.NewGuid()));
        await _patientRepository.DidNotReceive().SoftDeleteAsync(Arg.Any<Guid>());
    }
}

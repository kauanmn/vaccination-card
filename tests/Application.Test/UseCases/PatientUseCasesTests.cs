using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Application.UseCases.Patients;
using Domain.Entities;
using Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace ApplicationTest.UseCases;

public class PatientUseCasesTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();

    [Fact]
    public async Task CreatePatient_PersistsAndReturnsResponse()
    {
        var useCase = new CreatePatient(_patientRepository);

        var response = await useCase.RunAsync(new CreatePatientRequest { Name = "Kauan" });

        Assert.Equal("Kauan", response.Name);
        Assert.NotEqual(Guid.Empty, response.Id);
        await _patientRepository.Received(1).CreateAsync(Arg.Is<Patient>(p => p.Name == "Kauan"));
    }

    [Fact]
    public async Task CreatePatient_WithInvalidName_ThrowsAndDoesNotPersist()
    {
        var useCase = new CreatePatient(_patientRepository);

        await Assert.ThrowsAsync<InvalidPatientException>(
            () => useCase.RunAsync(new CreatePatientRequest { Name = "" }));

        await _patientRepository.DidNotReceive().CreateAsync(Arg.Any<Patient>());
    }

    [Fact]
    public async Task GetPatientById_WhenExists_ReturnsResponse()
    {
        var patient = new Patient("Kauan");
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
    public async Task DeletePatient_WhenExists_SoftDeletes()
    {
        var patient = new Patient("Kauan");
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

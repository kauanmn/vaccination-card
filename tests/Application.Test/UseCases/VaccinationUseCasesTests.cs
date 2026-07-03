using Application.Dtos.Vaccinations;
using Application.Ports.Persistence.Repositories;
using Application.UseCases.Vaccinations;
using Domain.Entities;
using Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace ApplicationTest.UseCases;

public class VaccinationUseCasesTests
{
    private readonly IPatientRepository _patientRepository = Substitute.For<IPatientRepository>();
    private readonly IVaccineRepository _vaccineRepository = Substitute.For<IVaccineRepository>();
    private readonly IVaccinationRepository _vaccinationRepository = Substitute.For<IVaccinationRepository>();

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private RegisterVaccination BuildRegister() =>
        new(_patientRepository, _vaccineRepository, _vaccinationRepository);

    [Fact]
    public async Task Register_HappyPath_PersistsVaccinationAndReturnsCard()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", 3);
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        _vaccineRepository.GetByIdAsync(vaccine.Id).Returns(vaccine);

        var response = await BuildRegister().RunAsync(patient.Id,
            new RegisterVaccinationRequest { VaccineId = vaccine.Id, Dose = 1, ApplicationDate = Today });

        var vaccination = Assert.Single(response.Vaccinations);
        Assert.Equal(vaccine.Id, vaccination.VaccineId);
        Assert.Equal(1, vaccination.Dose);
        await _vaccinationRepository.Received(1).CreateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Register_WhenPatientMissing_ThrowsNotFound()
    {
        _patientRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Patient?)null);

        await Assert.ThrowsAsync<PatientNotFound>(() => BuildRegister().RunAsync(Guid.NewGuid(),
            new RegisterVaccinationRequest { VaccineId = Guid.NewGuid(), Dose = 1, ApplicationDate = Today }));

        await _vaccinationRepository.DidNotReceive().CreateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Register_WhenVaccineMissing_ThrowsNotFound()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        _vaccineRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Vaccine?)null);

        await Assert.ThrowsAsync<VaccineNotFound>(() => BuildRegister().RunAsync(patient.Id,
            new RegisterVaccinationRequest { VaccineId = Guid.NewGuid(), Dose = 1, ApplicationDate = Today }));

        await _vaccinationRepository.DidNotReceive().CreateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Register_WhenDoseOutOfOrder_ThrowsAndDoesNotPersist()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", 3);
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        _vaccineRepository.GetByIdAsync(vaccine.Id).Returns(vaccine);

        await Assert.ThrowsAsync<InvalidVaccinationException>(() => BuildRegister().RunAsync(patient.Id,
            new RegisterVaccinationRequest { VaccineId = vaccine.Id, Dose = 2, ApplicationDate = Today }));

        await _vaccinationRepository.DidNotReceive().CreateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Update_WhenBelongsToPatient_ChangesDateAndPersists()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", 3);
        var vaccination = patient.AddVaccination(vaccine, 1, Today);
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new UpdateVaccination(_patientRepository, _vaccinationRepository);

        var newDate = Today.AddDays(-5);
        var response = await useCase.RunAsync(patient.Id, vaccination.Id,
            new UpdateVaccinationRequest { ApplicationDate = newDate });

        var updated = Assert.Single(response.Vaccinations);
        Assert.Equal(newDate, updated.ApplicationDate);
        await _vaccinationRepository.Received(1).UpdateAsync(
            Arg.Is<Vaccination>(v => v.Id == vaccination.Id && v.ApplicationDate == newDate));
    }

    [Fact]
    public async Task Update_WhenPatientMissing_ThrowsNotFound()
    {
        _patientRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Patient?)null);
        var useCase = new UpdateVaccination(_patientRepository, _vaccinationRepository);

        await Assert.ThrowsAsync<PatientNotFound>(() => useCase.RunAsync(Guid.NewGuid(), Guid.NewGuid(),
            new UpdateVaccinationRequest { ApplicationDate = Today }));

        await _vaccinationRepository.DidNotReceive().UpdateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Update_WhenVaccinationNotOnPatientCard_ThrowsNotFound()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        _patientRepository.GetByIdAsync(patient.Id).Returns(patient);
        var useCase = new UpdateVaccination(_patientRepository, _vaccinationRepository);

        await Assert.ThrowsAsync<VaccinationNotFound>(() => useCase.RunAsync(patient.Id, Guid.NewGuid(),
            new UpdateVaccinationRequest { ApplicationDate = Today }));

        await _vaccinationRepository.DidNotReceive().UpdateAsync(Arg.Any<Vaccination>());
    }

    [Fact]
    public async Task Remove_WhenBelongsToPatient_SoftDeletes()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", 3);
        var vaccination = patient.AddVaccination(vaccine, 1, Today);
        _vaccinationRepository.GetByIdAsync(vaccination.Id).Returns(vaccination);
        var useCase = new RemoveVaccination(_vaccinationRepository);

        await useCase.RunAsync(patient.Id, vaccination.Id);

        await _vaccinationRepository.Received(1).SoftDeleteAsync(vaccination.Id);
    }

    [Fact]
    public async Task Remove_WhenMissing_ThrowsNotFound()
    {
        _vaccinationRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Vaccination?)null);
        var useCase = new RemoveVaccination(_vaccinationRepository);

        await Assert.ThrowsAsync<VaccinationNotFound>(() => useCase.RunAsync(Guid.NewGuid(), Guid.NewGuid()));
        await _vaccinationRepository.DidNotReceive().SoftDeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Remove_WhenBelongsToAnotherPatient_ThrowsNotFound()
    {
        var owner = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", 3);
        var vaccination = owner.AddVaccination(vaccine, 1, Today);
        _vaccinationRepository.GetByIdAsync(vaccination.Id).Returns(vaccination);
        var useCase = new RemoveVaccination(_vaccinationRepository);

        var otherPatientId = Guid.NewGuid();

        await Assert.ThrowsAsync<VaccinationNotFound>(() => useCase.RunAsync(otherPatientId, vaccination.Id));
        await _vaccinationRepository.DidNotReceive().SoftDeleteAsync(Arg.Any<Guid>());
    }
}

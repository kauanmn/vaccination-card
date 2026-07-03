using Domain.Entities;
using Domain.Exceptions;
using Xunit;

namespace DomainTest;

public class PatientTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Constructor_WithValidName_CreatesPatient()
    {
        var patient = new Patient("Kauan");

        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.Equal("Kauan", patient.Name);
        Assert.Empty(patient.Vaccinations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<InvalidPatientException>(() => new Patient(name!));
    }

    [Fact]
    public void AddVaccination_FirstDose_AddsToCard()
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        var vaccination = patient.AddVaccination(vaccine, dose: 1, Today);

        Assert.Single(patient.Vaccinations);
        Assert.Equal(patient.Id, vaccination.PatientId);
        Assert.Equal(vaccine.Id, vaccination.VaccineId);
        Assert.Equal(1, vaccination.Dose);
        Assert.Equal(Today, vaccination.ApplicationDate);
    }

    [Fact]
    public void AddVaccination_SequentialDoses_AllAccepted()
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        patient.AddVaccination(vaccine, dose: 1, Today);
        patient.AddVaccination(vaccine, dose: 2, Today);
        patient.AddVaccination(vaccine, dose: 3, Today);

        Assert.Equal(3, patient.Vaccinations.Count);
    }

    [Fact]
    public void AddVaccination_NullVaccine_Throws()
    {
        var patient = new Patient("Kauan");

        Assert.Throws<ArgumentNullException>(() => patient.AddVaccination(null!, dose: 1, Today));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    public void AddVaccination_DoseOutOfRange_Throws(int dose)
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        Assert.Throws<InvalidVaccinationException>(() => patient.AddVaccination(vaccine, dose, Today));
    }

    [Fact]
    public void AddVaccination_DuplicateDose_Throws()
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);
        patient.AddVaccination(vaccine, dose: 1, Today);

        Assert.Throws<InvalidVaccinationException>(() => patient.AddVaccination(vaccine, dose: 1, Today));
    }

    [Fact]
    public void AddVaccination_OutOfOrder_Throws()
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        Assert.Throws<InvalidVaccinationException>(() => patient.AddVaccination(vaccine, dose: 2, Today));
    }

    [Fact]
    public void AddVaccination_SkippingDose_Throws()
    {
        var patient = new Patient("Kauan");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);
        patient.AddVaccination(vaccine, dose: 1, Today);

        Assert.Throws<InvalidVaccinationException>(() => patient.AddVaccination(vaccine, dose: 3, Today));
    }

    [Fact]
    public void AddVaccination_DosesFromDifferentVaccines_AreIndependent()
    {
        var patient = new Patient("Kauan");
        var covid = new Vaccine("COVID-19", totalDoses: 3);
        var yellowFever = new Vaccine("Febre Amarela", totalDoses: 1);

        patient.AddVaccination(covid, dose: 1, Today);
        var yf = patient.AddVaccination(yellowFever, dose: 1, Today);

        Assert.Equal(2, patient.Vaccinations.Count);
        Assert.Equal(yellowFever.Id, yf.VaccineId);
    }
}

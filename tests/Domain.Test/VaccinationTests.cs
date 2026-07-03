using Domain.Entities;
using Xunit;

namespace DomainTest;

public class VaccinationTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void ChangeApplicationDate_UpdatesDate()
    {
        var patient = new Patient("Kauan", "kauan", "hash");
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);
        var vaccination = patient.AddVaccination(vaccine, dose: 1, Today);

        var newDate = Today.AddDays(-10);
        vaccination.ChangeApplicationDate(newDate);

        Assert.Equal(newDate, vaccination.ApplicationDate);
    }
}

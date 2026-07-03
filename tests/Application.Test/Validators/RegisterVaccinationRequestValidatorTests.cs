using Application.Dtos.Vaccinations;
using Application.Validators;
using Xunit;

namespace ApplicationTest.Validators;

public class RegisterVaccinationRequestValidatorTests
{
    private readonly RegisterVaccinationRequestValidator _validator = new();

    private static RegisterVaccinationRequest Valid() => new()
    {
        VaccineId = Guid.NewGuid(),
        Dose = 1,
        ApplicationDate = DateOnly.FromDateTime(DateTime.UtcNow)
    };

    [Fact]
    public void Valid_Request_Passes()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FutureApplicationDate_Fails()
    {
        var request = Valid() with { ApplicationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterVaccinationRequest.ApplicationDate));
    }

    [Fact]
    public void EmptyVaccineId_Fails()
    {
        var request = Valid() with { VaccineId = Guid.Empty };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterVaccinationRequest.VaccineId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveDose_Fails(int dose)
    {
        var request = Valid() with { Dose = dose };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterVaccinationRequest.Dose));
    }
}

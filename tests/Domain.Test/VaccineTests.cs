using Domain.Entities;
using Domain.Exceptions;
using Xunit;

namespace DomainTest;

public class VaccineTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesVaccine()
    {
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        Assert.NotEqual(Guid.Empty, vaccine.Id);
        Assert.Equal("COVID-19", vaccine.Name);
        Assert.Equal(3, vaccine.TotalDoses);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<InvalidVaccineException>(() => new Vaccine(name!, totalDoses: 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidTotalDoses_Throws(int totalDoses)
    {
        Assert.Throws<InvalidVaccineException>(() => new Vaccine("COVID-19", totalDoses));
    }

    [Fact]
    public void Constructor_WithNullTotalDoses_CreatesPeriodicVaccine()
    {
        var vaccine = new Vaccine("Influenza (Gripe)", totalDoses: null);

        Assert.Null(vaccine.TotalDoses);
        Assert.True(vaccine.IsPeriodic);
    }

    [Fact]
    public void Update_WithValidData_ChangesNameAndTotalDoses()
    {
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        vaccine.Update("COVID-19 (bivalente)", totalDoses: 4);

        Assert.Equal("COVID-19 (bivalente)", vaccine.Name);
        Assert.Equal(4, vaccine.TotalDoses);
    }

    [Fact]
    public void Update_WithNullTotalDoses_BecomesPeriodic()
    {
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        vaccine.Update("COVID-19", totalDoses: null);

        Assert.True(vaccine.IsPeriodic);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithInvalidName_ThrowsAndKeepsState(string? name)
    {
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        Assert.Throws<InvalidVaccineException>(() => vaccine.Update(name!, totalDoses: 3));
        Assert.Equal("COVID-19", vaccine.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithInvalidTotalDoses_ThrowsAndKeepsState(int totalDoses)
    {
        var vaccine = new Vaccine("COVID-19", totalDoses: 3);

        Assert.Throws<InvalidVaccineException>(() => vaccine.Update("COVID-19", totalDoses));
        Assert.Equal(3, vaccine.TotalDoses);
    }
}

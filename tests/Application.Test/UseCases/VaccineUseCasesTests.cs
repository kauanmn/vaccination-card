using Application.Dtos.Vaccines;
using Application.Ports.Persistence.Repositories;
using Application.UseCases.Vaccines;
using Domain.Entities;
using Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace ApplicationTest.UseCases;

public class VaccineUseCasesTests
{
    private readonly IVaccineRepository _vaccineRepository = Substitute.For<IVaccineRepository>();

    [Fact]
    public async Task CreateVaccine_PersistsAndReturnsResponse()
    {
        var useCase = new CreateVaccine(_vaccineRepository);

        var response = await useCase.RunAsync(new CreateVaccineRequest { Name = "COVID-19", TotalDoses = 3 });

        Assert.Equal("COVID-19", response.Name);
        Assert.Equal(3, response.TotalDoses);
        await _vaccineRepository.Received(1).CreateAsync(Arg.Is<Vaccine>(v => v.Name == "COVID-19" && v.TotalDoses == 3));
    }

    [Fact]
    public async Task CreateVaccine_WithInvalidTotalDoses_ThrowsAndDoesNotPersist()
    {
        var useCase = new CreateVaccine(_vaccineRepository);

        await Assert.ThrowsAsync<InvalidVaccineException>(
            () => useCase.RunAsync(new CreateVaccineRequest { Name = "X", TotalDoses = 0 }));

        await _vaccineRepository.DidNotReceive().CreateAsync(Arg.Any<Vaccine>());
    }

    [Fact]
    public async Task GetVaccineById_WhenExists_ReturnsResponse()
    {
        var vaccine = new Vaccine("COVID-19", 3);
        _vaccineRepository.GetByIdAsync(vaccine.Id).Returns(vaccine);
        var useCase = new GetVaccineById(_vaccineRepository);

        var response = await useCase.RunAsync(vaccine.Id);

        Assert.Equal(vaccine.Id, response.Id);
        Assert.Equal("COVID-19", response.Name);
    }

    [Fact]
    public async Task GetVaccineById_WhenMissing_ThrowsNotFound()
    {
        _vaccineRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Vaccine?)null);
        var useCase = new GetVaccineById(_vaccineRepository);

        await Assert.ThrowsAsync<VaccineNotFound>(() => useCase.RunAsync(Guid.NewGuid()));
    }
}

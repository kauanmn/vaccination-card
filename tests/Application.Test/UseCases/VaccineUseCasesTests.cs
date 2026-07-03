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

    [Fact]
    public async Task ListVaccines_MapsItemsAndPagingMetadata()
    {
        var vaccines = new List<Vaccine> { new("COVID-19", 3), new("Hepatite B", 3) };
        _vaccineRepository.ListAsync(1, 20).Returns(((IReadOnlyList<Vaccine>)vaccines, 5));
        var useCase = new ListVaccines(_vaccineRepository);

        var response = await useCase.RunAsync(page: 1, pageSize: 20);

        Assert.Equal(2, response.Items.Count);
        Assert.Equal("COVID-19", response.Items[0].Name);
        Assert.Equal(1, response.Page);
        Assert.Equal(20, response.PageSize);
        Assert.Equal(5, response.TotalCount);
        Assert.Equal(1, response.TotalPages);
    }

    [Fact]
    public async Task ListVaccines_ClampsNonPositivePagingToDefaults()
    {
        _vaccineRepository.ListAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(((IReadOnlyList<Vaccine>)new List<Vaccine>(), 0));
        var useCase = new ListVaccines(_vaccineRepository);

        var response = await useCase.RunAsync(page: 0, pageSize: 0);

        Assert.Equal(1, response.Page);
        Assert.Equal(20, response.PageSize);
        await _vaccineRepository.Received(1).ListAsync(1, 20);
    }

    [Fact]
    public async Task ListVaccines_CapsPageSizeAtMax()
    {
        _vaccineRepository.ListAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(((IReadOnlyList<Vaccine>)new List<Vaccine>(), 0));
        var useCase = new ListVaccines(_vaccineRepository);

        var response = await useCase.RunAsync(page: 1, pageSize: 500);

        Assert.Equal(100, response.PageSize);
        await _vaccineRepository.Received(1).ListAsync(1, 100);
    }
}

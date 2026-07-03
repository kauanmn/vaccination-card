using Application.Dtos.Vaccines;
using Application.Ports.Persistence.Repositories;
using Domain.Entities;

namespace Application.UseCases.Vaccines;

public class CreateVaccine
{
    private readonly IVaccineRepository _vaccineRepository;

    public CreateVaccine(IVaccineRepository vaccineRepository)
    {
        _vaccineRepository = vaccineRepository;
    }

    public async Task<VaccineResponse> RunAsync(CreateVaccineRequest request)
    {
        var vaccine = new Vaccine(request.Name, request.TotalDoses);

        await _vaccineRepository.CreateAsync(vaccine);

        return vaccine.ToResponse();
    }
}

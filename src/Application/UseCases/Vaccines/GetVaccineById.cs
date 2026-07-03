using Application.Dtos.Vaccines;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Vaccines;

public class GetVaccineById
{
    private readonly IVaccineRepository _vaccineRepository;

    public GetVaccineById(IVaccineRepository vaccineRepository)
    {
        _vaccineRepository = vaccineRepository;
    }

    public async Task<VaccineResponse> RunAsync(Guid id)
    {
        var vaccine = await _vaccineRepository.GetByIdAsync(id)
                      ?? throw new VaccineNotFound($"Vacina {id} não encontrada.");

        return vaccine.ToResponse();
    }
}

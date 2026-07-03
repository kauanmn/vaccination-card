using Application.Dtos.Vaccines;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Vaccines;

public class UpdateVaccine
{
    private readonly IVaccineRepository _vaccineRepository;
    private readonly IVaccinationRepository _vaccinationRepository;

    public UpdateVaccine(IVaccineRepository vaccineRepository, IVaccinationRepository vaccinationRepository)
    {
        _vaccineRepository = vaccineRepository;
        _vaccinationRepository = vaccinationRepository;
    }

    public async Task<VaccineResponse> RunAsync(Guid id, UpdateVaccineRequest request)
    {
        var vaccine = await _vaccineRepository.GetByIdAsync(id)
                      ?? throw new VaccineNotFound($"Vacina {id} não encontrada.");

        // Reduzir o total de doses abaixo da maior dose já aplicada invalidaria registros existentes.
        if (request.TotalDoses is int totalDoses)
        {
            var maxAppliedDose = await _vaccinationRepository.GetMaxDoseByVaccineAsync(id);
            if (maxAppliedDose is int max && max > totalDoses)
                throw new InvalidVaccineException(
                    $"Não é possível definir o total de doses como {totalDoses}: " +
                    $"já existe registro de dose {max} para esta vacina.");
        }

        vaccine.Update(request.Name, request.TotalDoses);

        await _vaccineRepository.UpdateAsync(vaccine);

        return vaccine.ToResponse();
    }
}

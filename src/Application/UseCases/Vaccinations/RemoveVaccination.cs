using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Vaccinations;

public class RemoveVaccination
{
    private readonly IVaccinationRepository _vaccinationRepository;

    public RemoveVaccination(IVaccinationRepository vaccinationRepository)
    {
        _vaccinationRepository = vaccinationRepository;
    }

    public async Task RunAsync(Guid patientId, Guid vaccinationId)
    {
        var vaccination = await _vaccinationRepository.GetByIdAsync(vaccinationId)
                          ?? throw new VaccinationNotFound($"Vacinação {vaccinationId} não encontrada.");

        if (vaccination.PatientId != patientId)
            throw new VaccinationNotFound(
                $"Vacinação {vaccinationId} não encontrada para o paciente {patientId}.");

        await _vaccinationRepository.SoftDeleteAsync(vaccinationId);
    }
}

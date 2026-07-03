using Application.Dtos.Patients;
using Application.Dtos.Vaccinations;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Vaccinations;

public class UpdateVaccination
{
    private readonly IPatientRepository _patientRepository;
    private readonly IVaccinationRepository _vaccinationRepository;

    public UpdateVaccination(
        IPatientRepository patientRepository,
        IVaccinationRepository vaccinationRepository)
    {
        _patientRepository = patientRepository;
        _vaccinationRepository = vaccinationRepository;
    }

    public async Task<PatientResponse> RunAsync(
        Guid patientId,
        Guid vaccinationId,
        UpdateVaccinationRequest request)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId)
                      ?? throw new PatientNotFound($"Paciente {patientId} não encontrado.");

        var vaccination = patient.Vaccinations.FirstOrDefault(v => v.Id == vaccinationId)
                          ?? throw new VaccinationNotFound(
                              $"Vacinação {vaccinationId} não encontrada para o paciente {patientId}.");

        vaccination.ChangeApplicationDate(request.ApplicationDate);

        await _vaccinationRepository.UpdateAsync(vaccination);

        return patient.ToResponse();
    }
}

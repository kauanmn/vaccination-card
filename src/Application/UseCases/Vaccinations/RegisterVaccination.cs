using Application.Dtos.Patients;
using Application.Dtos.Vaccinations;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Vaccinations;

public class RegisterVaccination
{
    private readonly IPatientRepository _patientRepository;
    private readonly IVaccineRepository _vaccineRepository;
    private readonly IVaccinationRepository _vaccinationRepository;

    public RegisterVaccination(
        IPatientRepository patientRepository,
        IVaccineRepository vaccineRepository,
        IVaccinationRepository vaccinationRepository)
    {
        _patientRepository = patientRepository;
        _vaccineRepository = vaccineRepository;
        _vaccinationRepository = vaccinationRepository;
    }

    public async Task<PatientResponse> RunAsync(Guid patientId, RegisterVaccinationRequest request)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId)
                      ?? throw new PatientNotFound($"Paciente {patientId} não encontrado.");

        var vaccine = await _vaccineRepository.GetByIdAsync(request.VaccineId)
                      ?? throw new VaccineNotFound($"Vacina {request.VaccineId} não encontrada.");

        var vaccination = patient.AddVaccination(vaccine, request.Dose, request.ApplicationDate);

        await _vaccinationRepository.CreateAsync(vaccination);

        return patient.ToResponse();
    }
}

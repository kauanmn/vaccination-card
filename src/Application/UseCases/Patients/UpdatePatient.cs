using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Patients;

public class UpdatePatient
{
    private readonly IPatientRepository _patientRepository;

    public UpdatePatient(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientResponse> RunAsync(Guid id, UpdatePatientRequest request)
    {
        var patient = await _patientRepository.GetByIdAsync(id)
                      ?? throw new PatientNotFound($"Paciente {id} não encontrado.");

        patient.Rename(request.Name);

        await _patientRepository.UpdateAsync(patient);

        return patient.ToResponse();
    }
}

using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Patients;

public class DeletePatient
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatient(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task RunAsync(Guid id)
    {
        var patient = await _patientRepository.GetByIdAsync(id)
                      ?? throw new PatientNotFound($"Paciente {id} não encontrado.");

        await _patientRepository.SoftDeleteAsync(patient.Id);
    }
}

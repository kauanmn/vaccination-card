
using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Domain.Exceptions;

namespace Application.UseCases.Patients;

public class GetPatientById
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientById(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientResponse> RunAsync(Guid id)
    {
        var patient = await _patientRepository.GetByIdAsync(id)
                      ?? throw new PatientNotFound($"Patient {id} not found.");

        return patient.ToResponse();
    }
}

using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;
using Domain.Entities;

namespace Application.UseCases.Patients;

public class CreatePatient
{
    private readonly IPatientRepository _patientRepository;

    public CreatePatient(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientResponse> RunAsync(CreatePatientRequest request)
    {
        var patient = new Patient(request.Name);

        await _patientRepository.CreateAsync(patient);

        return patient.ToResponse();
    }
}
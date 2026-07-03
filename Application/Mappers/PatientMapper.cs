using Application.Dtos.Patients;
using Application.Dtos.Vaccinations;
using Domain.Entities;

public static class PatientMapper
{
    public static PatientResponse ToResponse(this Patient patient) => new()
    {
        Id = patient.Id,
        Name = patient.Name,
        Vaccinations = patient.Vaccinations
            .Select(v => new VaccinationResponse
            {
                Id = v.Id,
                VaccineId = v.VaccineId,
                Dose = v.Dose,
                ApplicationDate = v.ApplicationDate
            })
            .ToList()
    };
}
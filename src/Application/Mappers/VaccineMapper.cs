using Application.Dtos.Vaccines;
using Domain.Entities;

public static class VaccineMapper
{
    public static VaccineResponse ToResponse(this Vaccine vaccine) => new()
    {
        Id = vaccine.Id,
        Name = vaccine.Name,
        TotalDoses = vaccine.TotalDoses
    };
}

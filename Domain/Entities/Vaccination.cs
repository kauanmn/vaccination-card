namespace Domain.Entities;

public class Vaccination
{
    public required Guid Id { get; set; }
    public required Guid PatientId { get; set; }
    public required Guid VaccineId { get; set; }
    public required int Dose { get; set; }
    public required DateOnly ApplicationDate { get; set; }
}
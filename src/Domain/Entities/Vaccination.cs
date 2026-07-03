namespace Domain.Entities;

public class Vaccination
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid VaccineId { get; private set; }
    public int Dose { get; private set; }
    public DateOnly ApplicationDate { get; private set; }
    
    internal Vaccination(Guid patientId, Guid vaccineId, int dose, DateOnly applicationDate)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        VaccineId = vaccineId;
        Dose = dose;
        ApplicationDate = applicationDate;
    }

    private Vaccination() { }

    public void ChangeApplicationDate(DateOnly applicationDate)
    {
        ApplicationDate = applicationDate;
    }
}

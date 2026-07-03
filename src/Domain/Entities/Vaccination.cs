namespace Domain.Entities;

public class Vaccination
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid VaccineId { get; private set; }
    public int Dose { get; private set; }
    public DateOnly ApplicationDate { get; private set; }

    // Criada apenas pela raiz de agregação (Patient.AddVaccination),
    // garantindo que as invariantes de dose sejam sempre aplicadas.
    internal Vaccination(Guid patientId, Guid vaccineId, int dose, DateOnly applicationDate)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        VaccineId = vaccineId;
        Dose = dose;
        ApplicationDate = applicationDate;
    }

    private Vaccination() { }
}

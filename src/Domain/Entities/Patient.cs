using Domain.Exceptions;

namespace Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<Vaccination> _vaccinations = [];
    public IReadOnlyCollection<Vaccination> Vaccinations => _vaccinations.AsReadOnly();

    public Patient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPatientException();

        Id = Guid.NewGuid();
        Name = name;
    }

    private Patient() { }

    public Vaccination AddVaccination(Vaccine vaccine, int dose, DateOnly applicationDate)
    {
        ArgumentNullException.ThrowIfNull(vaccine);

        if (dose < 1 || dose > vaccine.TotalDoses)
            throw new InvalidVaccinationException(
                $"Dose deve estar entre 1 e {vaccine.TotalDoses} para a vacina {vaccine.Name}.");

        var appliedDoses = _vaccinations
            .Where(v => v.VaccineId == vaccine.Id)
            .Select(v => v.Dose)
            .ToList();

        if (appliedDoses.Contains(dose))
            throw new InvalidVaccinationException(
                $"Dose {dose} da vacina {vaccine.Name} já registrada.");

        var expectedNext = appliedDoses.Count == 0 ? 1 : appliedDoses.Max() + 1;
        if (dose != expectedNext)
            throw new InvalidVaccinationException(
                $"Dose fora de ordem: próxima dose esperada para a vacina {vaccine.Name} é {expectedNext}.");

        var vaccination = new Vaccination(Id, vaccine.Id, dose, applicationDate);
        _vaccinations.Add(vaccination);
        return vaccination;
    }
}
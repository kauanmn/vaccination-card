using Domain.Exceptions;

namespace Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }

    private readonly List<Vaccination> _vaccinations = [];
    public IReadOnlyCollection<Vaccination> Vaccinations => _vaccinations.AsReadOnly();

    public Patient(string name, string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPatientException();

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidPatientException("Usuário do paciente é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPatientException("Senha do paciente é obrigatória.");

        Id = Guid.NewGuid();
        Name = name;
        Username = username;
        PasswordHash = passwordHash;
    }

    private Patient() { }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidPatientException();

        Name = name;
    }

    public Vaccination AddVaccination(Vaccine vaccine, int dose, DateOnly applicationDate)
    {
        ArgumentNullException.ThrowIfNull(vaccine);

        if (dose < 1)
            throw new InvalidVaccinationException(
                $"Dose deve ser maior que zero para a vacina {vaccine.Name}.");

        if (vaccine.TotalDoses is int total && dose > total)
            throw new InvalidVaccinationException(
                $"Dose deve estar entre 1 e {total} para a vacina {vaccine.Name}.");

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
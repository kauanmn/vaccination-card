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
}
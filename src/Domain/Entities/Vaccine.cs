using Domain.Exceptions;

namespace Domain.Entities;

public class Vaccine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int TotalDoses { get; private set; }

    public Vaccine(string name, int totalDoses)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidVaccineException("Nome da vacina é obrigatório.");

        if (totalDoses < 1)
            throw new InvalidVaccineException("Total de doses deve ser maior que zero.");

        Id = Guid.NewGuid();
        Name = name;
        TotalDoses = totalDoses;
    }

    private Vaccine() { }
}

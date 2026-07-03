using Domain.Exceptions;

namespace Domain.Entities;

public class Vaccine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    /// <summary>
    /// Número total de doses do esquema vacinal.
    /// <c>null</c> indica uma vacina periódica (sem limite de doses, ex.: gripe anual).
    /// </summary>
    public int? TotalDoses { get; private set; }

    public bool IsPeriodic => TotalDoses is null;

    public Vaccine(string name, int? totalDoses)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidVaccineException("Nome da vacina é obrigatório.");

        if (totalDoses is < 1)
            throw new InvalidVaccineException("Total de doses deve ser maior que zero.");

        Id = Guid.NewGuid();
        Name = name;
        TotalDoses = totalDoses;
    }

    private Vaccine() { }
}

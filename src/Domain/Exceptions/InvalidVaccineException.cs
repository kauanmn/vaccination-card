namespace Domain.Exceptions;

public class InvalidVaccineException(string message = "Vacina inválida") : InvalidEntityException(message)
{
}

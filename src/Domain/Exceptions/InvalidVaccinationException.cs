namespace Domain.Exceptions;

public class InvalidVaccinationException(string message = "Vacinação inválida") : InvalidEntityException(message)
{
}

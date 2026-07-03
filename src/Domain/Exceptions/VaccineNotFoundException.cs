namespace Domain.Exceptions;

public class VaccineNotFound(string message = "Vacina não encontrada") : NotFoundException(message)
{
}

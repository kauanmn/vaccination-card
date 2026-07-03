namespace Domain.Exceptions;

public class VaccinationNotFound(string message = "Registro de vacinação não encontrado") : NotFoundException(message)
{
}

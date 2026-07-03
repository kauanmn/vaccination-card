namespace Domain.Exceptions;

public class PatientNotFound(string message = "Paciente não encontrado") : NotFoundException(message)
{
}
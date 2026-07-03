namespace Domain.Exceptions;

public class InvalidPatientException(string message = "Paciente inválido") : InvalidEntityException(message)
{
}
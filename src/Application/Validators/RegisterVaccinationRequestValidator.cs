using Application.Dtos.Vaccinations;
using FluentValidation;

namespace Application.Validators;

public class RegisterVaccinationRequestValidator : AbstractValidator<RegisterVaccinationRequest>
{
    public RegisterVaccinationRequestValidator()
    {
        RuleFor(x => x.VaccineId)
            .NotEmpty().WithMessage("Identificador da vacina é obrigatório.");

        RuleFor(x => x.Dose)
            .GreaterThan(0).WithMessage("Dose deve ser maior que zero.");

        RuleFor(x => x.ApplicationDate)
            .NotEqual(default(DateOnly)).WithMessage("Data de aplicação é obrigatória.")
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Data de aplicação não pode ser futura.");
    }
}

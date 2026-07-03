using Application.Dtos.Vaccinations;
using FluentValidation;

namespace Application.Validators;

public class UpdateVaccinationRequestValidator : AbstractValidator<UpdateVaccinationRequest>
{
    public UpdateVaccinationRequestValidator()
    {
        RuleFor(x => x.ApplicationDate)
            .NotEqual(default(DateOnly)).WithMessage("Data de aplicação é obrigatória.")
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Data de aplicação não pode ser futura.");
    }
}

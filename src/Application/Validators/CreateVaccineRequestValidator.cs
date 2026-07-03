using Application.Dtos.Vaccines;
using FluentValidation;

namespace Application.Validators;

public class CreateVaccineRequestValidator : AbstractValidator<CreateVaccineRequest>
{
    public CreateVaccineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.TotalDoses)
            .GreaterThan(0).WithMessage("Total de doses deve ser maior que zero.");
    }
}

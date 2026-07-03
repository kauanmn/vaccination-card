using Application.Dtos.Auth;
using FluentValidation;

namespace Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Usuário é obrigatório.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}

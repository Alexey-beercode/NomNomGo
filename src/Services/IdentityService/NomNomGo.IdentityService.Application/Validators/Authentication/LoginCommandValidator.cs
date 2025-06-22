using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Login;

namespace NomNomGo.IdentityService.Application.Validators.Authentication
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(v => v.Login)
                .NotEmpty().WithMessage("Логин не может быть пустым")
                .MaximumLength(100).WithMessage("Логин не может превышать 100 символов");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Пароль не может быть пустым")
                .MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов");
        }
    }
}
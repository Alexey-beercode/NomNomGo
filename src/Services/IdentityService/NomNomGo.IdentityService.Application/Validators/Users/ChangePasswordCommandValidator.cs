using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.ChangePassword;

namespace NomNomGo.IdentityService.Application.Validators.Users
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(v => v.CurrentPassword)
                .NotEmpty().WithMessage("Текущий пароль не может быть пустым");

            RuleFor(v => v.NewPassword)
                .NotEmpty().WithMessage("Новый пароль не может быть пустым")
                .MinimumLength(8).WithMessage("Новый пароль должен быть не менее 8 символов")
                .Matches("[A-Z]").WithMessage("Новый пароль должен содержать хотя бы одну заглавную букву")
                .Matches("[a-z]").WithMessage("Новый пароль должен содержать хотя бы одну строчную букву")
                .Matches("[0-9]").WithMessage("Новый пароль должен содержать хотя бы одну цифру")
                .Matches("[^a-zA-Z0-9]").WithMessage("Новый пароль должен содержать хотя бы один специальный символ")
                .NotEqual(v => v.CurrentPassword).WithMessage("Новый пароль должен отличаться от текущего");
        }
    }
}
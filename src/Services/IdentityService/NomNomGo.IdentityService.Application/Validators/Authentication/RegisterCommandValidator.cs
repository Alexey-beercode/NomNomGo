using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.Authentication.Commands.Register;

namespace NomNomGo.IdentityService.Application.Validators.Authentication
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("Email не может быть пустым")
                .EmailAddress().WithMessage("Некорректный формат Email")
                .MaximumLength(255).WithMessage("Email не может превышать 255 символов");

            RuleFor(v => v.Username)
                .NotEmpty().WithMessage("Имя пользователя не может быть пустым")
                .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов")
                .MaximumLength(100).WithMessage("Имя пользователя не может превышать 100 символов")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Имя пользователя может содержать только буквы, цифры и символ подчеркивания");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Пароль не может быть пустым")
                .MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов")
                .Matches("[a-zA-Z]").WithMessage("Пароль должен содержать хотя бы одну букву")
                .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру");
            // Убрали требования к заглавным буквам и спецсимволам

            RuleFor(v => v.PhoneNumber)
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Некорректный формат номера телефона")
                .MaximumLength(20).WithMessage("Номер телефона не может превышать 20 символов")
                .When(v => !string.IsNullOrEmpty(v.PhoneNumber));
        }
    }
}
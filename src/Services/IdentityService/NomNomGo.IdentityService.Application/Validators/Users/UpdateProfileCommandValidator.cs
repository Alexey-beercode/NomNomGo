using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.Users.Commands.UpdateProfile;

namespace NomNomGo.IdentityService.Application.Validators.Users
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(v => v.Email)
                .EmailAddress().WithMessage("Некорректный формат Email")
                .MaximumLength(255).WithMessage("Email не может превышать 255 символов")
                .When(v => !string.IsNullOrEmpty(v.Email));

            RuleFor(v => v.Username)
                .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов")
                .MaximumLength(100).WithMessage("Имя пользователя не может превышать 100 символов")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Имя пользователя может содержать только буквы, цифры и символ подчеркивания")
                .When(v => !string.IsNullOrEmpty(v.Username));

            RuleFor(v => v.PhoneNumber)
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Некорректный формат номера телефона")
                .MaximumLength(20).WithMessage("Номер телефона не может превышать 20 символов")
                .When(v => !string.IsNullOrEmpty(v.PhoneNumber));
        }
    }
}
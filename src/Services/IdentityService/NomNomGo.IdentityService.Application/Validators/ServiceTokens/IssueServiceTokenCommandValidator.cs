using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.ServiceTokens.IssueServiceToken.Commands;

namespace NomNomGo.IdentityService.Application.Validators.ServiceTokens
{
    public class IssueServiceTokenCommandValidator : AbstractValidator<IssueServiceTokenCommand>
    {
        public IssueServiceTokenCommandValidator()
        {
            RuleFor(v => v.ClientId)
                .NotEmpty().WithMessage("Client ID не может быть пустым");

            RuleFor(v => v.ClientSecret)
                .NotEmpty().WithMessage("Client Secret не может быть пустым");

            RuleFor(v => v.Scopes)
                .NotEmpty().WithMessage("Необходимо указать хотя бы одну область доступа (scope)");
        }
    }
}
using FluentValidation;
using NomNomGo.IdentityService.Application.UseCases.Users.Queries.Get;

namespace NomNomGo.IdentityService.Application.Validators.Users
{
    public class GetUserListQueryValidator : AbstractValidator<GetUserListQuery>
    {
        private readonly string[] _allowedSortFields = { "username", "email", "createdAt" };

        public GetUserListQueryValidator()
        {
            RuleFor(v => v.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть больше или равен 1");

            RuleFor(v => v.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Размер страницы должен быть больше или равен 1")
                .LessThanOrEqualTo(100).WithMessage("Размер страницы не может превышать 100");

            RuleFor(v => v.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || _allowedSortFields.Contains(sortBy.ToLower()))
                .WithMessage($"Поле сортировки должно быть одним из: {string.Join(", ", _allowedSortFields)}");
        }
    }
}
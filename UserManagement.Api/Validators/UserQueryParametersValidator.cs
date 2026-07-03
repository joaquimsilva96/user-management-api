using FluentValidation;
using UserManagement.Api.Models.Requests;

namespace UserManagement.Api.Validators;

public sealed class UserQueryParametersValidator : AbstractValidator<UserQueryParameters>
{
    public UserQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(parameters => parameters.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(parameters => parameters.Search)
            .MaximumLength(200)
            .When(parameters => parameters.Search is not null);
    }
}

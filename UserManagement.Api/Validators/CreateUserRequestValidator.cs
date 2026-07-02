using FluentValidation;
using UserManagement.Api.Models.Requests;

namespace UserManagement.Api.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .NotEmpty()
            .MaximumLength(200)
            .EmailAddress();
    }
}

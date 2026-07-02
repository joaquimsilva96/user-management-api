using FluentValidation;
using UserManagement.Api.Models.Requests;

namespace UserManagement.Api.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
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

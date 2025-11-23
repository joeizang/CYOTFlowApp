using System;
using FlowApplicationApp.ViewModels.Auditions;
using FluentValidation;

namespace FlowApplicationApp.Infrastructure.Validations;

public class CreateAuditionerValidator : AbstractValidator<CreateAuditionerInputModel>
{
    public CreateAuditionerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required.")
            .MaximumLength(100).WithMessage("Last Name cannot exceed 100 characters.");

        RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("You need to tell us a bit about yourself.")
            .MaximumLength(50).WithMessage("Your Bio cannot exceed 1000 characters.");
    }
}

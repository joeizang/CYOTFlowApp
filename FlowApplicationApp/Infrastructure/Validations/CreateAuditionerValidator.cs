using System;
using FlowApplicationApp.ViewModels.Auditions;
using FluentValidation;

namespace FlowApplicationApp.Infrastructure.Validations;

public class CreateAuditionerValidator : AbstractValidator<CreateAuditionerInputModel>
{
    public CreateAuditionerValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(5).WithMessage("First Name must be at least 5 characters long.")
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .MinimumLength(10).WithMessage("Email must be at least 10 characters long.")
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("Last Name must be at least 2 characters long.")
            .NotEmpty().WithMessage("Last Name is required.")
            .MaximumLength(100).WithMessage("Last Name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("A valid phone number is required. Preferrably a WhatsApp number.");

        RuleFor(x => x.Bio)
            .MinimumLength(10).WithMessage("Your Bio must be at least 10 characters long.")
            .NotEmpty().WithMessage("You need to tell us a bit about yourself.")
            .MaximumLength(1000).WithMessage("Your Bio cannot exceed 1000 characters.");
        
        RuleFor(x => x.HowTheyStartedHearingGod)
            .MinimumLength(3).WithMessage("This field must be at least 3 characters long.")
            .MaximumLength(500).WithMessage("This field cannot exceed 500 characters.")
            .When(x => x.HearsGod);
        RuleFor(x => x.CoverSpeech)
            .MinimumLength(20)
            .MaximumLength(1000)
            .NotEmpty()
            .WithMessage("You must tell us why you want to join flow. What is your motivation to join flow.");
        
        RuleFor(x => x.ProfileImage)
            .Must(file =>
            {
                var allowedExtensions = new[] { ".png", ".webp" };
                var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                return allowedExtensions.Contains(fileExtension);
            }).WithMessage("accepted images types have the .png or .webp extensions");
        RuleFor(x => x.ProfileImage)
            .Must(file => file.Length <= 750 * 1024) // 0.75 MB
            .WithMessage("Profile image size must not be larger than 750 KB.");
    }
}

using FluentValidation;
using MasterMind.API.Models.DTOs.Auth;

namespace MasterMind.API.Validators;

/// <summary>
/// Validator for OTP request DTO
/// </summary>
public class OtpRequestValidator : AbstractValidator<OtpRequestDto>
{
    public OtpRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required")
            .MaximumLength(100).WithMessage("Identifier must not exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(x => x.ToLower() == "mobile" || x.ToLower() == "email")
            .WithMessage("Type must be 'mobile' or 'email'");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required")
            .Must(BeValidPurpose).WithMessage("Invalid purpose. Valid values: login, registration, password_reset, verification");

        // Conditional validation based on type
        When(x => x.Type.ToLower() == "mobile", () =>
        {
            RuleFor(x => x.Identifier)
                .Matches(@"^[6-9]\d{9}$|^91[6-9]\d{9}$")
                .WithMessage("Invalid Indian mobile number format");
        });

        When(x => x.Type.ToLower() == "email", () =>
        {
            RuleFor(x => x.Identifier)
                .EmailAddress().WithMessage("Invalid email format");
        });
    }

    private static bool BeValidPurpose(string purpose)
    {
        var validPurposes = new[] { "login", "registration", "register", "password_reset", "reset", "verification", "verify_email", "verify_mobile" };
        return validPurposes.Contains(purpose.ToLower());
    }
}

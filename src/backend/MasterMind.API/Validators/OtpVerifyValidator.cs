using FluentValidation;
using MasterMind.API.Models.DTOs.Auth;

namespace MasterMind.API.Validators;

/// <summary>
/// Validator for OTP verification DTO
/// </summary>
public class OtpVerifyValidator : AbstractValidator<OtpVerifyDto>
{
    public OtpVerifyValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required")
            .MaximumLength(100).WithMessage("Identifier must not exceed 100 characters");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required")
            .Length(4, 6).WithMessage("OTP must be 4-6 digits")
            .Matches(@"^\d+$").WithMessage("OTP must contain only digits");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required");

        // Validate registration details when purpose is registration
        When(x => x.Purpose.ToLower() == "registration" || x.Purpose.ToLower() == "register", () =>
        {
            RuleFor(x => x.RegistrationDetails)
                .NotNull().WithMessage("Registration details are required for registration");

            When(x => x.RegistrationDetails != null, () =>
            {
                RuleFor(x => x.RegistrationDetails!.FirstName)
                    .NotEmpty().WithMessage("First name is required")
                    .Length(2, 50).WithMessage("First name must be 2-50 characters")
                    .Matches(@"^[a-zA-Z\s]+$").WithMessage("First name can only contain letters and spaces");

                RuleFor(x => x.RegistrationDetails!.LastName)
                    .NotEmpty().WithMessage("Last name is required")
                    .Length(2, 50).WithMessage("Last name must be 2-50 characters")
                    .Matches(@"^[a-zA-Z\s]+$").WithMessage("Last name can only contain letters and spaces");

                RuleFor(x => x.RegistrationDetails!.Role)
                    .Must(BeValidRole).WithMessage("Invalid role. Valid values: Parent, Teacher");
            });
        });
    }

    private static bool BeValidRole(string role)
    {
        var validRoles = new[] { "Parent", "Teacher" };
        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for verifying OTP
/// </summary>
public class OtpVerifyDto
{
    /// <summary>
    /// Mobile number or email that received the OTP
    /// </summary>
    [Required(ErrorMessage = "Identifier (mobile/email) is required")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The OTP code to verify
    /// </summary>
    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "OTP must be 4-6 digits")]
    public string Otp { get; set; } = string.Empty;

    /// <summary>
    /// Purpose of OTP verification
    /// </summary>
    [Required(ErrorMessage = "Purpose is required")]
    public string Purpose { get; set; } = "login";

    /// <summary>
    /// User details for registration (only required for registration purpose)
    /// </summary>
    public RegistrationDetailsDto? RegistrationDetails { get; set; }

    /// <summary>
    /// Device information for token generation
    /// </summary>
    public string? DeviceInfo { get; set; }
}

/// <summary>
/// DTO for registration details during OTP verification
/// </summary>
public class RegistrationDetailsDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid mobile number format")]
    public string? Mobile { get; set; }

    /// <summary>
    /// Role to assign: "Parent", "Teacher" (Admin can only be assigned by existing Admin)
    /// </summary>
    public string Role { get; set; } = "Parent";
}

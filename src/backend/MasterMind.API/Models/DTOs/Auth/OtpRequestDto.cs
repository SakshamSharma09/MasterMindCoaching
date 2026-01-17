using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for requesting OTP
/// </summary>
public class OtpRequestDto
{
    /// <summary>
    /// Mobile number or email to send OTP
    /// </summary>
    [Required(ErrorMessage = "Identifier (mobile/email) is required")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Type of identifier: "mobile" or "email"
    /// </summary>
    [Required(ErrorMessage = "Identifier type is required")]
    [RegularExpression("^(mobile|email)$", ErrorMessage = "Type must be 'mobile' or 'email'")]
    public string Type { get; set; } = "mobile";

    /// <summary>
    /// Purpose of OTP: "login", "registration", "password_reset", "verification"
    /// </summary>
    [Required(ErrorMessage = "Purpose is required")]
    public string Purpose { get; set; } = "login";
}

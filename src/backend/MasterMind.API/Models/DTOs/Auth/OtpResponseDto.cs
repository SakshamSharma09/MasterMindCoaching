namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for OTP request response
/// </summary>
public class OtpResponseDto
{
    /// <summary>
    /// Whether the OTP was sent successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Masked identifier (e.g., ****1234 for mobile)
    /// </summary>
    public string? MaskedIdentifier { get; set; }

    /// <summary>
    /// OTP expiry time in seconds
    /// </summary>
    public int ExpirySeconds { get; set; }

    /// <summary>
    /// Whether this is a new user (for registration flow)
    /// </summary>
    public bool IsNewUser { get; set; }

    /// <summary>
    /// Retry allowed after (in seconds)
    /// </summary>
    public int? RetryAfterSeconds { get; set; }
}

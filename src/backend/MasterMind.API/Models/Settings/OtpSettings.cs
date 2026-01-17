namespace MasterMind.API.Models.Settings;

/// <summary>
/// OTP configuration settings
/// </summary>
public class OtpSettings
{
    public const string SectionName = "Otp";

    /// <summary>
    /// OTP expiry time in minutes
    /// </summary>
    public int ExpiryMinutes { get; set; } = 5;

    /// <summary>
    /// OTP length (number of digits)
    /// </summary>
    public int Length { get; set; } = 6;

    /// <summary>
    /// Maximum verification attempts allowed
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Cooldown period in seconds before allowing resend
    /// </summary>
    public int ResendCooldownSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum OTP requests per hour per identifier
    /// </summary>
    public int MaxRequestsPerHour { get; set; } = 5;
}

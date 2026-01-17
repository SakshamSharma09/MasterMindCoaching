namespace MasterMind.API.Models.Entities;

/// <summary>
/// OTP record for email/mobile verification
/// </summary>
public class OtpRecord : BaseEntity
{
    public string Identifier { get; set; } = string.Empty; // Email or Mobile
    public string OtpCode { get; set; } = string.Empty;
    public OtpType Type { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public int AttemptCount { get; set; } = 0;
    public int? UserId { get; set; }

    // Navigation properties
    public User? User { get; set; }
}

public enum OtpType
{
    Email,
    Mobile
}

public enum OtpPurpose
{
    Login,
    Registration,
    PasswordReset,
    EmailVerification,
    MobileVerification
}

namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for quick login (bypasses OTP for demo accounts)
/// </summary>
public class QuickLoginDto
{
    /// <summary>
    /// Email of the demo account
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
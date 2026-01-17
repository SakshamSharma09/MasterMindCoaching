namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for authentication response
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Whether authentication was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// JWT access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Access token expiry time (UTC)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Error code for specific error handling
    /// </summary>
    public string? ErrorCode { get; set; }
}

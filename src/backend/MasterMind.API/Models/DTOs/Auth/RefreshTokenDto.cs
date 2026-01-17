using System.ComponentModel.DataAnnotations;

namespace MasterMind.API.Models.DTOs.Auth;

/// <summary>
/// DTO for refreshing access token
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// The refresh token to use for obtaining new access token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

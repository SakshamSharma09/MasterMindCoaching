using MasterMind.API.Models.DTOs.Auth;

namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<OtpResponseDto> RequestOtpAsync(OtpRequestDto request);
    Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request);
    Task<AuthResponseDto> LoginWithPasswordAsync(PasswordLoginDto request);
    Task<AuthResponseDto> QuickLoginAsync(string email);
    Task<AuthResponseDto> SetPasswordAsync(int userId, SetPasswordDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request);
    Task<bool> LogoutAsync(int userId, string? refreshToken = null);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}

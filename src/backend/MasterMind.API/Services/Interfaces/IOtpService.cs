using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// OTP service interface for generating and validating OTPs
/// </summary>
public interface IOtpService
{
    Task<string> GenerateOtpAsync(string identifier, OtpType type, OtpPurpose purpose, int? userId = null);
    Task<bool> ValidateOtpAsync(string identifier, string otp, OtpPurpose purpose);
    Task InvalidateOtpAsync(string identifier, OtpPurpose purpose);
}

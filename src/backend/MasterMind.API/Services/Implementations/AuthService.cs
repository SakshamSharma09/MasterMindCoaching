using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly MasterMindDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        MasterMindDbContext context,
        IOtpService otpService,
        IJwtService jwtService,
        IUserService userService,
        ISmsService smsService,
        IEmailService emailService,
        IOptions<OtpSettings> otpSettings,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _otpService = otpService;
        _jwtService = jwtService;
        _userService = userService;
        _smsService = smsService;
        _emailService = emailService;
        _otpSettings = otpSettings.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OtpResponseDto> RequestOtpAsync(OtpRequestDto request)
    {
        try
        {
            var isMobile = request.Type.ToLower() == "mobile";
            var identifier = request.Identifier.Trim();

            // Validate identifier format
            if (isMobile && !_smsService.IsValidMobileNumber(identifier))
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Invalid mobile number format"
                };
            }

            if (!isMobile && !_emailService.IsValidEmail(identifier))
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "Invalid email format"
                };
            }

            // Check if user exists
            var existingUser = await _userService.GetByIdentifierAsync(identifier);
            var isNewUser = existingUser == null;

            // Parse purpose
            var purpose = ParseOtpPurpose(request.Purpose);

            // For login, user must exist
            if (purpose == OtpPurpose.Login && isNewUser)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "User not found. Please register first.",
                    IsNewUser = true
                };
            }

            // For registration, user must not exist
            if (purpose == OtpPurpose.Registration && !isNewUser)
            {
                return new OtpResponseDto
                {
                    Success = false,
                    Message = "User already exists. Please login instead."
                };
            }

            // Generate and send OTP
            var otpType = isMobile ? OtpType.Mobile : OtpType.Email;
            var otp = await _otpService.GenerateOtpAsync(
                identifier, 
                otpType, 
                purpose, 
                existingUser?.Id);

            _logger.LogInformation("OTP requested for {Identifier} ({Type}) - Purpose: {Purpose}", 
                MaskIdentifier(identifier, isMobile), request.Type, purpose);

            return new OtpResponseDto
            {
                Success = true,
                Message = $"OTP sent to your {(isMobile ? "mobile" : "email")}",
                MaskedIdentifier = MaskIdentifier(identifier, isMobile),
                ExpirySeconds = _otpSettings.ExpiryMinutes * 60,
                IsNewUser = isNewUser,
                RetryAfterSeconds = _otpSettings.ResendCooldownSeconds
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "OTP request failed for {Identifier}", request.Identifier);
            return new OtpResponseDto
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting OTP for {Identifier}", request.Identifier);
            return new OtpResponseDto
            {
                Success = false,
                Message = "An error occurred. Please try again."
            };
        }
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(OtpVerifyDto request)
    {
        try
        {
            var identifier = request.Identifier.Trim();
            var purpose = ParseOtpPurpose(request.Purpose);

            // Validate OTP
            var isValid = await _otpService.ValidateOtpAsync(identifier, request.Otp, purpose);
            if (!isValid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired OTP",
                    ErrorCode = "INVALID_OTP"
                };
            }

            // Get or create user
            var user = await _userService.GetByIdentifierAsync(identifier);
            var isMobile = !identifier.Contains('@');

            if (user == null)
            {
                // Registration flow - create new user
                if (purpose != OtpPurpose.Registration)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                if (request.RegistrationDetails == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Registration details are required",
                        ErrorCode = "REGISTRATION_DETAILS_REQUIRED"
                    };
                }

                user = await _userService.CreateUserAsync(request.RegistrationDetails, identifier, isMobile);
            }
            else
            {
                // Update verification status
                if (isMobile && !user.IsMobileVerified)
                {
                    await _userService.VerifyMobileAsync(user.Id);
                }
                else if (!isMobile && !user.IsEmailVerified)
                {
                    await _userService.VerifyEmailAsync(user.Id);
                }
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Your account has been deactivated. Please contact support.",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            // Generate tokens
            var roles = await _userService.GetUserRolesAsync(user.Id);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken(GetClientIp());

            // Save refresh token
            refreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("User {UserId} authenticated successfully via OTP", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Identifier}", request.Identifier);
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during authentication",
                ErrorCode = "AUTH_ERROR"
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto request)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid refresh token",
                    ErrorCode = "INVALID_REFRESH_TOKEN"
                };
            }

            if (!refreshToken.IsActive)
            {
                // Token has been revoked or expired - revoke all tokens for this user
                await RevokeAllUserTokensAsync(refreshToken.UserId, "Attempted use of revoked token");
                
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Refresh token has expired or been revoked",
                    ErrorCode = "TOKEN_EXPIRED"
                };
            }

            var user = refreshToken.User;
            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Account has been deactivated",
                    ErrorCode = "ACCOUNT_DEACTIVATED"
                };
            }

            // Rotate refresh token
            var newRefreshToken = _jwtService.GenerateRefreshToken(GetClientIp());
            newRefreshToken.UserId = user.Id;

            // Revoke old token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = GetClientIp();
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.ReasonRevoked = "Replaced by new token";

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            // Generate new access token
            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var userDto = await _userService.ToUserDtoAsync(user);

            _logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Tokens refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred while refreshing token",
                ErrorCode = "REFRESH_ERROR"
            };
        }
    }

    public async Task<bool> LogoutAsync(int userId, string? refreshToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Revoke specific token
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

                if (token != null)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = GetClientIp();
                    token.ReasonRevoked = "User logout";
                }
            }
            else
            {
                // Revoke all tokens for user
                await RevokeAllUserTokensAsync(userId, "User logout (all devices)");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} logged out", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return null;
        return await _userService.ToUserDtoAsync(user);
    }

    private async Task RevokeAllUserTokensAsync(int userId, string reason)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = GetClientIp();
            token.ReasonRevoked = reason;
        }
    }

    private static OtpPurpose ParseOtpPurpose(string purpose)
    {
        return purpose.ToLower() switch
        {
            "login" => OtpPurpose.Login,
            "registration" or "register" => OtpPurpose.Registration,
            "password_reset" or "reset" => OtpPurpose.PasswordReset,
            "email_verification" or "verify_email" => OtpPurpose.EmailVerification,
            "mobile_verification" or "verify_mobile" => OtpPurpose.MobileVerification,
            _ => OtpPurpose.Login
        };
    }

    private static string MaskIdentifier(string identifier, bool isMobile)
    {
        if (isMobile)
        {
            // Mask mobile: show last 4 digits
            var digits = new string(identifier.Where(char.IsDigit).ToArray());
            if (digits.Length >= 4)
            {
                return "****" + digits.Substring(digits.Length - 4);
            }
        }
        else
        {
            // Mask email: show first 2 chars and domain
            var parts = identifier.Split('@');
            if (parts.Length == 2)
            {
                var localPart = parts[0];
                var domain = parts[1];
                var maskedLocal = localPart.Length > 2 
                    ? localPart.Substring(0, 2) + "***" 
                    : localPart + "***";
                return maskedLocal + "@" + domain;
            }
        }
        return "****";
    }

    private string GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "unknown";

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

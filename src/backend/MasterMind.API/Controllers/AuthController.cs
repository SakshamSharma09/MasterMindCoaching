using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

/// <summary>
/// Authentication controller for OTP-based login, registration, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "mm_access_token";
    private const string RefreshTokenCookieName = "mm_refresh_token";
    private readonly IAuthService _authService;
    private readonly IDeviceService _deviceService;
    private readonly ILogger<AuthController> _logger;
    private readonly MasterMindDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IDeviceService deviceService,
        ILogger<AuthController> logger,
        MasterMindDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _authService = authService;
        _deviceService = deviceService;
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Request OTP for login or registration
    /// </summary>
    /// <param name="request">OTP request details</param>
    /// <returns>OTP response with masked identifier and expiry info</returns>
    [HttpPost("otp/request")]
    [HttpPost("request-otp")] // Alias for backward compatibility
    [ProducesResponseType(typeof(OtpResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OtpResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OtpResponseDto), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OtpResponseDto>> RequestOtp([FromBody] OtpRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new OtpResponseDto
            {
                Success = false,
                Message = "Invalid request data"
            });
        }

        var result = await _authService.RequestOtpAsync(request);
        
        if (!result.Success)
        {
            if (result.Message.Contains("Too many"))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verify OTP and authenticate user
    /// </summary>
    /// <param name="request">OTP verification details</param>
    /// <returns>Authentication response with tokens and user info</returns>
    [HttpPost("otp/verify")]
    [HttpPost("verify-otp")] // Alias for backward compatibility
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> VerifyOtp([FromBody] OtpVerifyDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid request data",
                ErrorCode = "VALIDATION_ERROR"
            });
        }

        var result = await _authService.VerifyOtpAsync(request);
        
        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "INVALID_OTP" => Unauthorized(result),
                "USER_NOT_FOUND" => NotFound(result),
                "ACCOUNT_DEACTIVATED" => StatusCode(StatusCodes.Status403Forbidden, result),
                "ACCOUNT_NOT_PROVISIONED" => StatusCode(StatusCodes.Status403Forbidden, result),
                _ => BadRequest(result)
            };
        }

        WriteAuthCookies(result);
        return Ok(result);
    }

    /// <summary>
    /// Admin login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> LoginWithPassword([FromBody] PasswordLoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = "Invalid request data", ErrorCode = "VALIDATION_ERROR" });

        var result = await _authService.LoginWithPasswordAsync(request);
        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "INVALID_CREDENTIALS" => Unauthorized(result),
                "ACCOUNT_DEACTIVATED" => StatusCode(StatusCodes.Status403Forbidden, result),
                _ => BadRequest(result)
            };
        }
        WriteAuthCookies(result);
        return Ok(result);
    }

    /// <summary>
    /// Quick access login for test/demo accounts (bypasses OTP)
    /// </summary>
    [HttpPost("quick-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> QuickLogin([FromBody] QuickLoginDto request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Email is required", ErrorCode = "VALIDATION_ERROR" });
        }

        var allowedEmails = new[] { "admin@mastermind.com", "teacher@mastermind.com", "parent@mastermind.com" };
        if (!allowedEmails.Contains(request.Email.ToLower()))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Invalid quick access email", ErrorCode = "INVALID_EMAIL" });
        }

        var result = await _authService.QuickLoginAsync(request.Email);
        if (result.Success)
        {
            WriteAuthCookies(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Set password for the current authenticated user
    /// </summary>
    [HttpPost("set-password")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> SetPassword([FromBody] SetPasswordDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = "Invalid request data" });

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new AuthResponseDto { Success = false, Message = "Invalid token" });

        var result = await _authService.SetPasswordAsync(userId.Value, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("invitations/{token}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ValidateInvitation(string token)
    {
        var invitation = await FindValidInvitationAsync(token);
        if (invitation == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "This invitation is invalid, expired, or has already been used"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Invitation is valid",
            Data = new
            {
                invitation.ExpiresAt,
                Mobile = MaskMobile(invitation.User.Mobile),
                Name = $"{invitation.User.FirstName} {invitation.User.LastName}".Trim(),
                AccountType = GetInvitationAccountType(invitation.User)
            }
        });
    }

    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Use a password of at least 8 characters" });
        }

        var invitation = await FindValidInvitationAsync(request.Token);
        if (invitation == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "This invitation is invalid, expired, or has already been used"
            });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailInUse = await _context.Users.AnyAsync(u =>
            !u.IsDeleted && u.Id != invitation.UserId && u.Email.ToLower() == normalizedEmail);
        if (emailInUse)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "This email is already linked to another account"
            });
        }

        invitation.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invitation.User.Email = normalizedEmail;
        invitation.User.IsActive = true;
        invitation.User.IsEmailVerified = false;
        invitation.User.UpdatedAt = DateTime.UtcNow;
        invitation.UsedAt = DateTime.UtcNow;
        invitation.UpdatedAt = DateTime.UtcNow;

        var accountType = GetInvitationAccountType(invitation.User);
        if (accountType == "Teacher")
        {
            var teachers = await _context.Teachers
                .Where(t => !t.IsDeleted && (t.UserId == invitation.UserId || t.Mobile == invitation.User.Mobile))
                .ToListAsync();
            foreach (var teacher in teachers)
            {
                teacher.UserId = invitation.UserId;
                teacher.Email = normalizedEmail;
                teacher.UpdatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var normalizedMobile = NormalizeMobile(invitation.User.Mobile);
            var students = await _context.Students
                .Where(s => !s.IsDeleted && s.ParentUserId == invitation.UserId)
                .ToListAsync();
            var sameMobileStudents = (await _context.Students
                    .Where(s => !s.IsDeleted && s.ParentUserId != invitation.UserId)
                    .ToListAsync())
                .Where(s => NormalizeMobile(s.ParentMobile) == normalizedMobile);
            foreach (var student in students.Concat(sameMobileStudents))
            {
                student.ParentUserId = invitation.UserId;
                student.ParentEmail = normalizedEmail;
                student.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ??
            "https://victorious-glacier-0e6507000.6.azurestaticapps.net").TrimEnd('/');
        var loginUrl = $"{frontendBaseUrl}/login?mobile={Uri.EscapeDataString(invitation.User.Mobile)}";
        try
        {
            await _emailService.SendEmailAsync(
                normalizedEmail,
                $"Your MasterMind Coaching {accountType.ToLowerInvariant()} account is ready",
                $"""
                <p>Namaste,</p>
                <p>Your {accountType.ToLowerInvariant()} account password has been set successfully.</p>
                <p><a href="{loginUrl}">Open MasterMind Coaching and sign in</a> using your registered mobile number.</p>
                <p>This email can also be used for secure OTP access and password recovery.</p>
                <p>— MasterMind Coaching Classes</p>
                """);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{AccountType} account {UserId} was activated, but confirmation email failed", accountType, invitation.UserId);
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Password set successfully. Sign in with your mobile number.",
            Data = new { Mobile = invitation.User.Mobile }
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("token/refresh")]
    [HttpPost("refresh-token")] // Alias for backward compatibility
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken) &&
            Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieRefreshToken))
        {
            request.RefreshToken = cookieRefreshToken;
            ModelState.Clear();
            TryValidateModel(request);
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "Invalid request data",
                ErrorCode = "VALIDATION_ERROR"
            });
        }

        var result = await _authService.RefreshTokenAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        WriteAuthCookies(result);
        return Ok(result);
    }

    /// <summary>
    /// Logout user and revoke refresh token
    /// </summary>
    /// <param name="request">Optional refresh token to revoke specific session</param>
    /// <returns>Logout status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto? request = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken) &&
            Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieRefreshToken))
        {
            refreshToken = cookieRefreshToken;
        }

        var success = await _authService.LogoutAsync(userId.Value, refreshToken);
        
        if (success)
        {
            ClearAuthCookies();
            return Ok(new { message = "Logged out successfully" });
        }

        return BadRequest(new { message = "Logout failed" });
    }

    /// <summary>
    /// Logout from all devices
    /// </summary>
    /// <returns>Logout status</returns>
    [HttpPost("logout/all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var success = await _authService.LogoutAsync(userId.Value);
        
        if (success)
        {
            ClearAuthCookies();
            return Ok(new { message = "Logged out from all devices successfully" });
        }

        return BadRequest(new { message = "Logout failed" });
    }

    /// <summary>
    /// Get current authenticated user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var user = await _authService.GetCurrentUserAsync(userId.Value);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Check if user is authenticated (token validation)
    /// </summary>
    /// <returns>Authentication status</returns>
    [HttpGet("check")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult CheckAuth()
    {
        var userId = GetCurrentUserId();
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        return Ok(new
        {
            authenticated = true,
            userId,
            roles
        });
    }

    /// <summary>
    /// Get user's registered devices
    /// </summary>
    /// <returns>List of user devices</returns>
    [HttpGet("devices")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserDevices()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var devices = await _deviceService.GetUserDevicesAsync(userId.Value);
            var deviceDtos = devices.Select(d => new
            {
                d.DeviceId,
                d.DeviceName,
                d.DeviceType,
                d.IsTrusted,
                d.LastUsedAt,
                d.CreatedAt
            }).ToList();

            return Ok(deviceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user devices");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Trust a device for OTP-free login
    /// </summary>
    /// <param name="request">Device trust request</param>
    /// <returns>Trust status</returns>
    [HttpPost("device/trust")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrustDevice([FromBody] TrustDeviceRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _deviceService.TrustDeviceAsync(userId.Value, request.DeviceId);
            
            return Ok(new { message = "Device trusted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trusting device");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Revoke device access
    /// </summary>
    /// <param name="request">Device revoke request</param>
    /// <returns>Revoke status</returns>
    [HttpPost("device/revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeDevice([FromBody] RevokeDeviceRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _deviceService.RevokeDeviceAsync(userId.Value, request.DeviceId);
            
            return Ok(new { message = "Device revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking device");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
            ?? User.FindFirst("sub") 
            ?? User.FindFirst("uid");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    private async Task<AccountInvitation?> FindValidInvitationAsync(string? rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken.Trim())));
        var now = DateTime.UtcNow;
        return await _context.AccountInvitations
            .Include(i => i.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(i => i.TokenHash == hash && i.UsedAt == null &&
                i.RevokedAt == null && i.ExpiresAt > now && !i.IsDeleted && !i.User.IsDeleted &&
                !i.User.UserRoles.Any(ur => ur.Role.Name == "Admin") &&
                i.User.UserRoles.Any(ur => ur.Role.Name == "Parent" || ur.Role.Name == "Teacher"));
    }

    private static string GetInvitationAccountType(User user) =>
        user.UserRoles.Any(ur => ur.Role.Name == "Teacher") ? "Teacher" : "Parent";

    private static string MaskMobile(string mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length <= 4 ? digits : $"{new string('•', digits.Length - 4)}{digits[^4..]}";
    }

    private static string NormalizeMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal)
            ? digits[2..]
            : digits;
    }

    private void WriteAuthCookies(AuthResponseDto result)
    {
        if (!result.Success || string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            return;
        }

        var accessTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.ExpiresAt ?? DateTime.UtcNow.AddHours(1),
            IsEssential = true
        };

        var refreshTokenCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(30),
            IsEssential = true
        };

        Response.Cookies.Append(AccessTokenCookieName, result.AccessToken, accessTokenCookieOptions);
        Response.Cookies.Append(RefreshTokenCookieName, result.RefreshToken, refreshTokenCookieOptions);
    }

    private void ClearAuthCookies()
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            IsEssential = true
        };

        Response.Cookies.Delete(AccessTokenCookieName, options);
        Response.Cookies.Delete(RefreshTokenCookieName, options);
    }
}

// DTOs for device management
public class TrustDeviceRequest
{
    public string DeviceId { get; set; } = string.Empty;
}

public class RevokeDeviceRequest
{
    public string DeviceId { get; set; } = string.Empty;
}

public class AcceptInvitationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

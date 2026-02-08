using System.Security.Claims;
using MasterMind.API.Models.DTOs.Auth;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterMind.API.Controllers;

/// <summary>
/// Authentication controller for OTP-based login, registration, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDeviceService _deviceService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IDeviceService deviceService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Request OTP for login or registration
    /// </summary>
    /// <param name="request">OTP request details</param>
    /// <returns>OTP response with masked identifier and expiry info</returns>
    [HttpPost("otp/request")]
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
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access and refresh tokens</returns>
    [HttpPost("token/refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
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

        var result = await _authService.RefreshTokenAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

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

        var success = await _authService.LogoutAsync(userId.Value, request?.RefreshToken);
        
        if (success)
        {
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

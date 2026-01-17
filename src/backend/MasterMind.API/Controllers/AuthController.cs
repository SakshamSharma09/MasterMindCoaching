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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
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

using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public AccountController(MasterMindDbContext context)
    {
        _context = context;
    }

    [HttpGet("security")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<ApiResponse<object>>> GetSecurityDetails()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid account" });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Account security details retrieved",
            Data = new
            {
                Email = IsPlaceholderEmail(user.Email) ? string.Empty : user.Email,
                PrimaryMobile = user.Mobile,
                SecondaryMobile = user.SecondaryMobile,
                user.IsEmailVerified
            }
        });
    }

    [HttpPut("security/email")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRecoveryEmail([FromBody] UpdateRecoveryEmailDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Enter a valid email address" });
        }

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid account" });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => !u.IsDeleted && u.Id != user.Id && u.Email.ToLower() == normalizedEmail))
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "This email is already linked to another account" });
        }

        user.Email = normalizedEmail;
        user.IsEmailVerified = false;
        user.UpdatedAt = DateTime.UtcNow;
        var students = await _context.Students
            .Where(s => !s.IsDeleted && s.ParentUserId == user.Id)
            .ToListAsync();
        foreach (var student in students)
        {
            student.ParentEmail = normalizedEmail;
            student.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Recovery email updated. Use Email OTP to verify and securely access your account."
        });
    }

    [HttpPost("deletion-request")]
    public async Task<ActionResult<ApiResponse<object>>> RequestAuthenticatedDeletion(
        [FromBody] AccountDeletionRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid account" });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Account not found" });
        }

        return await CreateRequestAsync(user, user.Email, request.Reason);
    }

    [HttpPost("public-deletion-request")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> RequestPublicDeletion(
        [FromBody] PublicAccountDeletionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrMobile))
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Email or mobile number is required" });
        }

        var identifier = request.EmailOrMobile.Trim();
        var normalizedMobile = new string(identifier.Where(char.IsDigit).ToArray());
        var users = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();
        var user = users.FirstOrDefault(u =>
            u.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(normalizedMobile) &&
             new string(u.Mobile.Where(char.IsDigit).ToArray()).EndsWith(normalizedMobile, StringComparison.Ordinal)));

        // Always return the same response so this endpoint cannot be used to enumerate accounts.
        await CreateRequestAsync(user, identifier, request.Reason);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "If the account exists, the deletion request has been recorded. MasterMind Coaching will contact you about completion and any legally retained records."
        });
    }

    private async Task<ActionResult<ApiResponse<object>>> CreateRequestAsync(
        User? user,
        string identifier,
        string? reason)
    {
        var existing = await _context.AccountDeletionRequests.AnyAsync(r =>
            r.Status == "Pending" &&
            ((user != null && r.UserId == user.Id) || r.EmailOrMobile == identifier));
        if (!existing)
        {
            _context.AccountDeletionRequests.Add(new AccountDeletionRequest
            {
                UserId = user?.Id,
                EmailOrMobile = identifier,
                Reason = reason?.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Account deletion request recorded"
        });
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId)
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted)
            : null;
    }

    private static bool IsPlaceholderEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ||
        email.EndsWith("@placeholder.mastermind.local", StringComparison.OrdinalIgnoreCase);
}

public class AccountDeletionRequestDto
{
    public string? Reason { get; set; }
}

public class PublicAccountDeletionRequestDto : AccountDeletionRequestDto
{
    public string EmailOrMobile { get; set; } = string.Empty;
}

public class UpdateRecoveryEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;
using System.Text.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeachersController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ITeacherSalaryService _teacherSalaryService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(
        MasterMindDbContext context,
        ITeacherSalaryService teacherSalaryService,
        IBlobStorageService blobStorageService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<TeachersController> logger)
    {
        _context = context;
        _teacherSalaryService = teacherSalaryService;
        _blobStorageService = blobStorageService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    // GET: api/Teachers
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<Teacher>>>> GetTeachers([FromQuery] int? sessionId = null)
    {
        try
        {
            if (sessionId.HasValue)
            {
                var sessionExists = await _context.Sessions.AnyAsync(s => s.Id == sessionId.Value && !s.IsDeleted);
                if (!sessionExists)
                {
                    sessionId = null;
                }
            }

            if (!sessionId.HasValue)
            {
                var activeSession = await _context.Sessions.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            var teachers = await _context.Teachers
                .Where(t => !t.IsDeleted && (!sessionId.HasValue || t.SessionId == sessionId.Value))
                .Include(t => t.TeacherClasses)
                    .ThenInclude(tc => tc.Class)
                .Select(t => new Teacher
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    Mobile = t.Mobile,
                    DateOfBirth = t.DateOfBirth,
                    Specialization = t.Specialization,
                    Qualification = t.Qualification,
                    Subjects = t.Subjects,
                    ExperienceYears = t.ExperienceYears,
                    MonthlySalary = t.MonthlySalary,
                    JoiningDate = t.JoiningDate,
                    ProfileImageUrl = t.ProfileImageUrl,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    TeacherClasses = t.TeacherClasses
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Teacher>>
            {
                Success = true,
                Message = "Teachers retrieved successfully",
                Data = teachers
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Teacher>>
            {
                Success = false,
                Message = $"Error retrieving teachers: {ex.Message}",
                Data = null
            });
        }
    }

    // GET: api/Teachers/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<Teacher>>> GetTeacher(int id)
    {
        try
        {
            var teacher = await _context.Teachers
                .Where(t => t.Id == id && !t.IsDeleted)
                .Include(t => t.TeacherClasses)
                    .ThenInclude(tc => tc.Class)
                .Select(t => new Teacher
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    Mobile = t.Mobile,
                    DateOfBirth = t.DateOfBirth,
                    Specialization = t.Specialization,
                    Qualification = t.Qualification,
                    Subjects = t.Subjects,
                    ExperienceYears = t.ExperienceYears,
                    MonthlySalary = t.MonthlySalary,
                    JoiningDate = t.JoiningDate,
                    ProfileImageUrl = t.ProfileImageUrl,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    TeacherClasses = t.TeacherClasses
                })
                .FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound(new ApiResponse<Teacher>
                {
                    Success = false,
                    Message = "Teacher not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Teacher>
            {
                Success = true,
                Message = "Teacher retrieved successfully",
                Data = teacher
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Teacher>
            {
                Success = false,
                Message = $"Error retrieving teacher: {ex.Message}",
                Data = null
            });
        }
    }

    // POST: api/Teachers
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Teacher>>> CreateTeacher([FromBody] JsonElement teacherData)
    {
        try
        {
            // Get current active session
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);

            if (activeSession == null)
            {
                return BadRequest(new ApiResponse<Teacher>
                {
                    Success = false,
                    Message = "No active session found. Please activate a session first.",
                    Data = null
                });
            }

            var mobile = NormalizeMobile(teacherData.GetProperty("mobile").GetString());
            if (mobile.Length != 10)
            {
                return BadRequest(new ApiResponse<Teacher>
                {
                    Success = false,
                    Message = "A valid 10-digit teacher mobile number is required"
                });
            }

            var email = teacherData.TryGetProperty("email", out var emailElement)
                ? emailElement.GetString()?.Trim().ToLowerInvariant()
                : null;
            email = string.IsNullOrWhiteSpace(email) ? BuildTeacherPlaceholderEmail(mobile) : email;

            var accountConflict = await GetTeacherAccountConflictAsync(mobile, null);
            if (accountConflict != null)
            {
                return BadRequest(new ApiResponse<Teacher> { Success = false, Message = accountConflict });
            }

            if (!teacherData.TryGetProperty("joiningDate", out var joiningDateElement) ||
                !DateTime.TryParse(joiningDateElement.GetString(), out var joiningDate))
            {
                return BadRequest(new ApiResponse<Teacher> { Success = false, Message = "A valid teacher joining date is required" });
            }

            if (!teacherData.TryGetProperty("dateOfBirth", out var dateOfBirthElement) ||
                !DateTime.TryParse(dateOfBirthElement.GetString(), out var dateOfBirth) ||
                dateOfBirth.Date > DateTime.Today)
            {
                return BadRequest(new ApiResponse<Teacher> { Success = false, Message = "A valid teacher date of birth is required" });
            }

            // Create new teacher entity
            var teacher = new Teacher
            {
                FirstName = teacherData.GetProperty("firstName").GetString() ?? string.Empty,
                LastName = teacherData.GetProperty("lastName").GetString() ?? string.Empty,
                Email = email,
                Mobile = mobile,
                DateOfBirth = dateOfBirth.Date,
                Specialization = teacherData.GetProperty("specialization").GetString(),
                Qualification = teacherData.GetProperty("qualification").GetString(),
                
                // Handle Subjects property if it exists
                Subjects = teacherData.TryGetProperty("subjects", out JsonElement subjectsElement) ? subjectsElement.GetString() : null,
                
                ExperienceYears = teacherData.GetProperty("experienceYears").GetInt32(),
                MonthlySalary = teacherData.GetProperty("monthlySalary").GetDecimal(),
                JoiningDate = joiningDate,
                IsActive = teacherData.GetProperty("isActive").GetBoolean(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SessionId = activeSession.Id
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            await LinkTeacherUserAsync(teacher);
            await _teacherSalaryService.EnsureMonthlyObligationsAsync(activeSession.Id);

            // Handle class assignments if provided
            if (teacherData.TryGetProperty("classIds", out JsonElement classIdsElement))
            {
                var classIds = classIdsElement.EnumerateArray().Select(x => x.GetInt32()).ToList();
                foreach (var classId in classIds)
                {
                    var teacherClass = new TeacherClass
                    {
                        TeacherId = teacher.Id,
                        ClassId = classId
                    };
                    _context.TeacherClasses.Add(teacherClass);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id },
                new ApiResponse<Teacher>
                {
                    Success = true,
                    Message = "Teacher created successfully",
                    Data = teacher
                });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Teacher>
            {
                Success = false,
                Message = $"Error creating teacher: {ex.Message}",
                Data = null
            });
        }
    }

    // PUT: api/Teachers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeacher(int id, [FromBody] dynamic updateData)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherClasses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Teacher not found",
                    Data = null
                });
            }

            JsonElement teacherUpdate = updateData;
            var mobile = NormalizeMobile(teacherUpdate.GetProperty("mobile").GetString());
            if (mobile.Length != 10)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "A valid 10-digit teacher mobile number is required" });
            }

            var accountConflict = await GetTeacherAccountConflictAsync(mobile, teacher.UserId);
            if (accountConflict != null)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = accountConflict });
            }

            var submittedEmail = teacherUpdate.TryGetProperty("email", out var emailElement)
                ? emailElement.GetString()?.Trim().ToLowerInvariant()
                : null;

            // Update basic teacher properties
            teacher.FirstName = updateData.GetProperty("firstName").GetString();
            teacher.LastName = updateData.GetProperty("lastName").GetString();
            if (!string.IsNullOrWhiteSpace(submittedEmail) || IsPlaceholderEmail(teacher.Email))
            {
                teacher.Email = string.IsNullOrWhiteSpace(submittedEmail) ? BuildTeacherPlaceholderEmail(mobile) : submittedEmail;
            }
            teacher.Mobile = mobile;
            if (teacherUpdate.TryGetProperty("dateOfBirth", out JsonElement dateOfBirthElement) &&
                DateTime.TryParse(dateOfBirthElement.GetString(), out DateTime parsedDateOfBirth))
            {
                if (parsedDateOfBirth.Date > DateTime.Today)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Teacher date of birth cannot be in the future" });
                }
                teacher.DateOfBirth = parsedDateOfBirth.Date;
            }
            teacher.Specialization = updateData.GetProperty("specialization").GetString();
            teacher.Qualification = updateData.GetProperty("qualification").GetString();
            
            // Handle Subjects property if it exists
            if (updateData.TryGetProperty("subjects", out JsonElement subjectsElement))
            {
                teacher.Subjects = subjectsElement.GetString();
            }
            
            int experienceYears = updateData.GetProperty("experienceYears").GetInt32();
            decimal monthlySalary = updateData.GetProperty("monthlySalary").GetDecimal();
            teacher.ExperienceYears = experienceYears;
            teacher.MonthlySalary = monthlySalary;
            if (teacherUpdate.TryGetProperty("joiningDate", out JsonElement joiningDateElement) &&
                DateTime.TryParse(joiningDateElement.GetString(), out DateTime parsedJoiningDate))
            {
                teacher.JoiningDate = parsedJoiningDate;
            }
            if (teacherUpdate.TryGetProperty("isActive", out JsonElement activeElement))
            {
                teacher.IsActive = activeElement.GetBoolean();
            }
            teacher.UpdatedAt = DateTime.UtcNow;

            // Handle class assignments if provided
            if (updateData.TryGetProperty("classIds", out JsonElement classIdsElement))
            {
                // Remove existing class assignments
                var existingClasses = await _context.TeacherClasses.Where(tc => tc.TeacherId == id).ToListAsync();
                _context.TeacherClasses.RemoveRange(existingClasses);

                // Add new class assignments
                var classIds = classIdsElement.EnumerateArray().Select(x => x.GetInt32()).ToList();
                foreach (var classId in classIds)
                {
                    var teacherClass = new TeacherClass
                    {
                        TeacherId = id,
                        ClassId = classId
                    };
                    _context.TeacherClasses.Add(teacherClass);
                }
            }

            await _context.SaveChangesAsync();

            await LinkTeacherUserAsync(teacher);
            await _teacherSalaryService.EnsureMonthlyObligationsAsync(teacher.SessionId);

            return Ok(new ApiResponse<Teacher>
            {
                Success = true,
                Message = "Teacher updated successfully",
                Data = teacher
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error updating teacher: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPost("{id:int}/photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> UploadTeacherPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "No file uploaded" });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "File size exceeds 5MB limit" });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid file type. Allowed: jpg, jpeg, png, gif, webp"
            });
        }

        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (teacher == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Teacher not found" });
        }

        try
        {
            var existingBlobName = ExtractBlobNameFromUrl(teacher.ProfileImageUrl);
            if (!string.IsNullOrWhiteSpace(existingBlobName))
            {
                await _blobStorageService.DeletePhotoAsync(existingBlobName);
            }

            using var stream = file.OpenReadStream();
            var blobName = await _blobStorageService.UploadPhotoAsync(stream, file.FileName, file.ContentType);
            var photoUrl = _blobStorageService.GetPhotoUrl(blobName);

            teacher.ProfileImageUrl = photoUrl;
            teacher.UpdatedAt = DateTime.UtcNow;
            if (teacher.UserId.HasValue)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == teacher.UserId.Value && !u.IsDeleted);
                if (user != null)
                {
                    user.ProfileImageUrl = photoUrl;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Teacher photo uploaded successfully",
                Data = new { blobName, url = photoUrl }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for teacher {TeacherId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error uploading photo: {ex.Message}"
            });
        }
    }

    // POST: api/Teachers/5/invitation
    [HttpPost("{id:int}/invitation")]
    public async Task<ActionResult<ApiResponse<object>>> CreateTeacherInvitation(int id)
    {
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && t.IsActive);
        if (teacher == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Active teacher not found" });
        }

        var mobile = NormalizeMobile(teacher.Mobile);
        if (mobile.Length != 10)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Add a valid 10-digit teacher mobile number before sending an invitation" });
        }

        var conflict = await GetTeacherAccountConflictAsync(mobile, teacher.UserId);
        if (conflict != null)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = conflict });
        }

        await LinkTeacherUserAsync(teacher);
        if (!teacher.UserId.HasValue)
        {
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "The teacher account could not be created" });
        }

        var teacherUser = await _context.Users.FirstAsync(u => u.Id == teacher.UserId.Value);
        if (!string.IsNullOrWhiteSpace(teacherUser.PasswordHash) && !IsPlaceholderEmail(teacherUser.Email))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "This teacher has already set a password. They can use Email OTP for access and password recovery."
            });
        }

        var now = DateTime.UtcNow;
        var activeInvitations = await _context.AccountInvitations
            .Where(i => i.UserId == teacher.UserId.Value && i.UsedAt == null && i.RevokedAt == null && i.ExpiresAt > now)
            .ToListAsync();
        foreach (var activeInvitation in activeInvitations)
        {
            activeInvitation.RevokedAt = now;
            activeInvitation.UpdatedAt = now;
        }

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        var expiresAt = now.AddHours(72);
        _context.AccountInvitations.Add(new AccountInvitation
        {
            UserId = teacher.UserId.Value,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByUserId = GetCurrentUserId(),
            CreatedAt = now
        });
        await _context.SaveChangesAsync();

        var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ??
            "https://victorious-glacier-0e6507000.6.azurestaticapps.net").TrimEnd('/');
        var inviteUrl = $"{frontendBaseUrl}/accept-invitation?token={Uri.EscapeDataString(rawToken)}";
        var teacherName = $"{teacher.FirstName} {teacher.LastName}".Trim();
        var emailSent = false;
        if (!IsPlaceholderEmail(teacher.Email))
        {
            try
            {
                emailSent = await _emailService.SendEmailAsync(
                    teacher.Email,
                    "Set your MasterMind Coaching teacher password",
                    $"<p>Namaste {System.Net.WebUtility.HtmlEncode(teacherName)},</p><p>You have been invited to the MasterMind teacher app.</p><p><a href=\"{inviteUrl}\">Set your recovery email and password</a></p><p>This private link expires in 72 hours. Afterwards, sign in with your mobile number.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Teacher invitation {TeacherId} was created, but email delivery failed", teacher.Id);
            }
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = emailSent ? "Teacher invitation created and emailed" : "Teacher invitation created for WhatsApp sharing",
            Data = new
            {
                InviteUrl = inviteUrl,
                ExpiresAt = expiresAt,
                EmailSent = emailSent,
                PrimaryMobile = mobile,
                WhatsAppMessage = $"Namaste {teacherName}, use this private link to set your MasterMind teacher app password and recovery email: {inviteUrl} This link expires in 72 hours."
            }
        });
    }

    // DELETE: api/Teachers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Teacher not found",
                    Data = null
                });
            }

            teacher.IsDeleted = true;
            teacher.UpdatedAt = DateTime.UtcNow;

            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Teacher deleted successfully",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error deleting teacher: {ex.Message}",
                Data = null
            });
        }
    }

    private bool TeacherExists(int id)
    {
        return _context.Teachers.Any(e => e.Id == id && !e.IsDeleted);
    }

    private async Task LinkTeacherUserAsync(Teacher teacher)
    {
        var mobile = NormalizeMobile(teacher.Mobile);
        if (mobile.Length != 10)
        {
            return;
        }

        teacher.Mobile = mobile;
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
        var user = users.FirstOrDefault(u => NormalizeMobile(u.Mobile) == mobile);

        if (user == null)
        {
            user = new User
            {
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Mobile = mobile,
                Email = IsPlaceholderEmail(teacher.Email) ? BuildTeacherPlaceholderEmail(mobile) : teacher.Email.Trim().ToLowerInvariant(),
                IsActive = true,
                IsEmailVerified = false,
                IsMobileVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        user.FirstName = teacher.FirstName;
        user.LastName = teacher.LastName;
        user.Mobile = mobile;
        user.IsActive = teacher.IsActive;
        user.ProfileImageUrl = teacher.ProfileImageUrl;
        if (!IsPlaceholderEmail(teacher.Email))
        {
            user.Email = teacher.Email.Trim().ToLowerInvariant();
        }
        teacher.UserId = user.Id;
        teacher.UpdatedAt = DateTime.UtcNow;

        var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Teacher");
        if (teacherRole != null && !await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == teacherRole.Id))
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = teacherRole.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<string?> GetTeacherAccountConflictAsync(string mobile, int? linkedUserId)
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
        var user = users.FirstOrDefault(u => NormalizeMobile(u.Mobile) == mobile);
        if (user == null || user.Id == linkedUserId)
        {
            return null;
        }

        if (user.UserRoles.Any(ur => ur.Role.Name is "Admin" or "Parent"))
        {
            return "This mobile number belongs to an Admin or Parent account. Use a different teacher mobile number.";
        }

        var linkedToAnotherTeacher = await _context.Teachers.AnyAsync(t =>
            !t.IsDeleted && t.UserId == user.Id && (!linkedUserId.HasValue || user.Id != linkedUserId.Value));
        return linkedToAnotherTeacher
            ? "This mobile number is already linked to another teacher."
            : null;
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static string NormalizeMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal) ? digits[2..] : digits;
    }

    private static string BuildTeacherPlaceholderEmail(string mobile) =>
        $"teacher_{mobile}@placeholder.mastermind.local";

    private static bool IsPlaceholderEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) || email.EndsWith("@placeholder.mastermind.local", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractBlobNameFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var lastSegment = uri.Segments.LastOrDefault();
        return string.IsNullOrWhiteSpace(lastSegment)
            ? null
            : Uri.UnescapeDataString(lastSegment.Trim('/'));
    }
}

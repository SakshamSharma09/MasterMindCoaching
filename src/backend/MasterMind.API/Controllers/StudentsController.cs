using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class StudentsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<StudentsController> _logger;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public StudentsController(
        MasterMindDbContext context,
        ILogger<StudentsController> logger,
        IBlobStorageService blobStorageService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _blobStorageService = blobStorageService;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all students with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="classId">Optional class ID to filter students</param>
    /// <param name="sessionId">Optional session ID to filter students</param>
    /// <returns>Paginated list of students</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<Student>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<Student>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? classId = null,
        [FromQuery] int? sessionId = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // If no valid sessionId provided, use the active session
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
                var activeSession = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            // Start with a simple query first
            var query = _context.Students.Where(s => !s.IsDeleted);

            // Filter by session if provided
            if (sessionId.HasValue)
            {
                query = query.Where(s => s.SessionId == sessionId);
            }

            if (classId.HasValue)
            {
                query = query.Where(s => s.StudentClasses.Any(sc => sc.ClassId == classId.Value && sc.IsActive));
            }

            query = query.OrderBy(s => s.Id);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Use a simpler query to avoid SessionId issues, but include class information
            var students = await query
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PaginatedResult<Student>
            {
                Data = students,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Success = true,
                Message = "Students retrieved successfully"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students");
            return StatusCode(500, new PaginatedResult<Student>
            {
                Data = new List<Student>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0,
                Success = false,
                Message = $"Error retrieving students: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get a specific student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Student>>> GetStudent(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (student == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Student not found"
            });
        }

        return Ok(new ApiResponse<Student>
        {
            Success = true,
            Message = "Student retrieved successfully",
            Data = student
        });
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="student">Student data</param>
    /// <param name="sessionId">Optional session ID (defaults to active session)</param>
    /// <returns>Created student</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Student>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Student>>> CreateStudent(
        [FromBody] Student student,
        [FromQuery] int? sessionId = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid student data"
            });
        }

        if (string.IsNullOrWhiteSpace(student.ParentEmail))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Parent email is required"
            });
        }

        // If no sessionId provided, use the active session
        if (!sessionId.HasValue)
        {
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
            sessionId = activeSession?.Id;
        }

        // Assign the session ID
        student.SessionId = sessionId;
        student.CreatedAt = DateTime.UtcNow;
        student.IsActive = true;
        student.IsDeleted = false;

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        await LinkParentUserAsync(student);
        var invitationSent = await SendParentInvitationAsync(student);

        return CreatedAtAction(nameof(GetStudent),
            new { id = student.Id },
            new ApiResponse<Student>
            {
                Success = true,
                Message = invitationSent
                    ? "Student created and parent invitation sent successfully"
                    : "Student created. Parent invitation could not be sent; use Resend invitation.",
                Data = student
            });
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="student">Updated student data</param>
    /// <returns>Updated student</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Student>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Student>>> UpdateStudent(int id, [FromBody] Student student)
    {
        if (id != student.Id)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "ID mismatch"
            });
        }

        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (existingStudent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Student not found"
            });
        }

        // Update fields
        existingStudent.FirstName = student.FirstName;
        existingStudent.LastName = student.LastName;
        existingStudent.DateOfBirth = student.DateOfBirth;
        existingStudent.Gender = student.Gender;
        existingStudent.Address = student.Address;
        existingStudent.City = student.City;
        existingStudent.State = student.State;
        existingStudent.PinCode = student.PinCode;
        existingStudent.StudentMobile = student.StudentMobile;
        existingStudent.StudentEmail = student.StudentEmail;
        existingStudent.ProfileImageUrl = student.ProfileImageUrl;
        existingStudent.PhotoBlobName = student.PhotoBlobName;
        existingStudent.AdmissionNumber = student.AdmissionNumber;
        existingStudent.AdmissionDate = student.AdmissionDate;
        existingStudent.IsActive = student.IsActive;
        existingStudent.ParentName = student.ParentName;
        existingStudent.MotherName = student.MotherName;
        existingStudent.FatherName = student.FatherName;
        existingStudent.CurrentSchool = student.CurrentSchool;
        existingStudent.ParentMobile = student.ParentMobile;
        existingStudent.ParentEmail = student.ParentEmail;
        existingStudent.ParentOccupation = student.ParentOccupation;
        existingStudent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await LinkParentUserAsync(existingStudent);

        return Ok(new ApiResponse<Student>
        {
            Success = true,
            Message = "Student updated successfully",
            Data = existingStudent
        });
    }

    /// <summary>
    /// Map a student to a class
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="classId">Class ID</param>
    /// <returns>Mapping status</returns>
    [HttpPost("{studentId}/classes/{classId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> MapStudentToClass(int studentId, int classId)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted);

            if (student == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Class not found"
                });
            }

            // Check if mapping already exists
            var existingMapping = await _context.StudentClasses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.ClassId == classId && sc.IsActive);

            if (existingMapping != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student is already mapped to this class"
                });
            }

            // Create new mapping
            var studentClass = new StudentClass
            {
                StudentId = studentId,
                ClassId = classId,
                IsActive = true,
                EnrollmentDate = DateTime.UtcNow
            };

            if (classEntity.SessionId.HasValue)
            {
                student.SessionId = classEntity.SessionId;
            }

            _context.StudentClasses.Add(studentClass);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Student mapped to class successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping student to class");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error mapping student to class: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Remove student from class
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="classId">Class ID</param>
    /// <returns>Unmapping status</returns>
    [HttpDelete("{studentId}/classes/{classId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UnmapStudentFromClass(int studentId, int classId)
    {
        try
        {
            var studentClass = await _context.StudentClasses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.ClassId == classId && sc.IsActive);

            if (studentClass == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student-class mapping not found"
                });
            }

            studentClass.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Student unmapped from class successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unmapping student from class");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error unmapping student from class: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get all students available for mapping (unmapped or with inactive mappings)
    /// </summary>
    /// <param name="classId">Optional class ID to filter students not in this class</param>
    /// <returns>List of available students for mapping</returns>
    [HttpGet("available-for-mapping")]
    [ProducesResponseType(typeof(ApiResponse<List<Student>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Student>>>> GetAvailableStudentsForMapping([FromQuery] int? classId = null)
    {
        try
        {
            var query = _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .Where(s => !s.IsDeleted && s.IsActive);

            if (classId.HasValue)
            {
                // Prefer navigation-based predicate so EF translates to a single SQL query
                query = query.Where(s => !s.StudentClasses.Any(sc =>
                    sc.ClassId == classId.Value && sc.IsActive));
            }

            var students = await query
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();

            return Ok(new ApiResponse<List<Student>>
            {
                Success = true,
                Message = "Available students retrieved successfully",
                Data = students
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available students for mapping");
            return StatusCode(500, new ApiResponse<List<Student>>
            {
                Success = false,
                Message = $"Error retrieving available students: {ex.Message}",
                Data = new List<Student>()
            });
        }
    }

    /// <summary>
    /// Upload photo for a student
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="file">Photo file</param>
    /// <returns>Upload status with photo URL</returns>
    [HttpPost("{id}/photo")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> UploadStudentPhoto(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No file uploaded"
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "File size exceeds 5MB limit"
                });
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

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            // Delete old photo if exists
            var existingBlobName = student.PhotoBlobName;
            if (string.IsNullOrWhiteSpace(existingBlobName) && !string.IsNullOrWhiteSpace(student.ProfileImageUrl))
            {
                existingBlobName = ExtractBlobNameFromUrl(student.ProfileImageUrl);
            }

            if (!string.IsNullOrWhiteSpace(existingBlobName))
            {
                await _blobStorageService.DeletePhotoAsync(existingBlobName);
            }

            // Upload new photo
            using var stream = file.OpenReadStream();
            var blobName = await _blobStorageService.UploadPhotoAsync(stream, file.FileName, file.ContentType);
            var photoUrl = _blobStorageService.GetPhotoUrl(blobName);

            // Update student record
            student.PhotoBlobName = blobName;
            student.ProfileImageUrl = photoUrl;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Photo uploaded successfully",
                Data = new
                {
                    blobName,
                    url = photoUrl
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for student {StudentId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Error uploading photo: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Delete a student (soft delete)
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Delete status</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStudent(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (student == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Student not found"
            });
        }

        student.IsDeleted = true;
        student.UpdatedAt = DateTime.UtcNow;

        var removableFees = await _context.StudentFees
            .Where(sf => sf.StudentId == id && !sf.IsDeleted &&
                !sf.Payments.Any(p => !p.IsDeleted))
            .ToListAsync();
        foreach (var fee in removableFees)
        {
            fee.IsDeleted = true;
            fee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Student deleted successfully"
        });
    }

    [HttpPost("{id}/parent-invitation")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> ResendParentInvitation(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (student == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Student not found" });
        }

        if (string.IsNullOrWhiteSpace(student.ParentEmail))
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Add a parent email before sending an invitation" });
        }

        await LinkParentUserAsync(student);
        var sent = await SendParentInvitationAsync(student);
        if (!sent)
        {
            return StatusCode(503, new ApiResponse<object>
            {
                Success = false,
                Message = "Invitation was created, but email delivery failed. Please try again."
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Parent invitation sent successfully"
        });
    }

    private static string? ExtractBlobNameFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var lastSegment = uri.Segments.LastOrDefault();
        if (string.IsNullOrWhiteSpace(lastSegment))
        {
            return null;
        }

        return Uri.UnescapeDataString(lastSegment.Trim('/'));
    }

    private async Task LinkParentUserAsync(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.ParentEmail))
        {
            return;
        }

        var normalizedEmail = student.ParentEmail.Trim().ToLowerInvariant();
        var normalizedMobile = NormalizeMobile(student.ParentMobile);
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync();
        var user = users.FirstOrDefault(u =>
            u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(normalizedMobile) && NormalizeMobile(u.Mobile) == normalizedMobile));

        if (user == null)
        {
            var displayName = !string.IsNullOrWhiteSpace(student.ParentName)
                ? student.ParentName.Trim()
                : student.MotherName?.Trim() ?? student.FatherName?.Trim() ?? "Parent";
            var nameParts = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user = new User
            {
                Email = normalizedEmail,
                Mobile = normalizedMobile,
                FirstName = nameParts.FirstOrDefault() ?? "Parent",
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                IsActive = true,
                IsEmailVerified = false,
                IsMobileVerified = !string.IsNullOrWhiteSpace(normalizedMobile),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        if (student.ParentUserId != user.Id)
        {
            student.ParentUserId = user.Id;
            student.UpdatedAt = DateTime.UtcNow;
        }

        var parentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Parent");
        if (parentRole != null && !await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == parentRole.Id))
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = parentRole.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<bool> SendParentInvitationAsync(Student student)
    {
        if (!student.ParentUserId.HasValue || string.IsNullOrWhiteSpace(student.ParentEmail))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var activeInvitations = await _context.AccountInvitations
            .Where(i => i.UserId == student.ParentUserId.Value &&
                i.UsedAt == null && i.RevokedAt == null && i.ExpiresAt > now)
            .ToListAsync();
        foreach (var invitation in activeInvitations)
        {
            invitation.RevokedAt = now;
            invitation.UpdatedAt = now;
        }

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        _context.AccountInvitations.Add(new AccountInvitation
        {
            UserId = student.ParentUserId.Value,
            StudentId = student.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(72),
            CreatedByUserId = GetCurrentUserId(),
            CreatedAt = now
        });
        await _context.SaveChangesAsync();

        var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ??
            "https://victorious-glacier-0e6507000.6.azurestaticapps.net").TrimEnd('/');
        var inviteUrl = $"{frontendBaseUrl}/accept-invitation?token={Uri.EscapeDataString(rawToken)}";
        var studentName = $"{student.FirstName} {student.LastName}".Trim();
        var body = $"""
            <p>Namaste,</p>
            <p>You have been invited to join the MasterMind Coaching Classes parent app for <strong>{System.Net.WebUtility.HtmlEncode(studentName)}</strong>.</p>
            <p><a href="{inviteUrl}">Set your password</a></p>
            <p>This private, single-use link expires in 72 hours. After setting the password, sign in with your mobile number.</p>
            <p>— MasterMind Coaching Classes</p>
            """;
        return await _emailService.SendEmailAsync(
            student.ParentEmail.Trim(),
            "Set your MasterMind Coaching parent password",
            body);
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static string NormalizeMobile(string? mobile)
    {
        var digits = new string((mobile ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal))
        {
            return digits[2..];
        }

        return digits;
    }
}

// DTOs for API responses
public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

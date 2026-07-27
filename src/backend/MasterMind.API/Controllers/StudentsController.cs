using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using MasterMind.API.Utilities;
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
            var parentUserIds = students
                .Where(s => s.ParentUserId.HasValue)
                .Select(s => s.ParentUserId!.Value)
                .Distinct()
                .ToList();
            var onboardedParentIds = parentUserIds.Count == 0
                ? new HashSet<int>()
                : (await _context.Users
                    .AsNoTracking()
                    .Where(u => parentUserIds.Contains(u.Id) &&
                        u.PasswordHash != null && u.PasswordHash != string.Empty && !u.IsDeleted)
                    .Select(u => u.Id)
                    .ToListAsync())
                    .ToHashSet();
            foreach (var student in students)
            {
                student.ParentOnboarded = student.ParentUserId.HasValue &&
                    onboardedParentIds.Contains(student.ParentUserId.Value);
            }

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
    /// Download every non-deleted student record across all academic sessions as an Excel workbook.
    /// </summary>
    [HttpGet("export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportStudents()
    {
        var students = await _context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Include(s => s.Session)
            .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
            .OrderBy(s => s.SessionId)
            .ThenBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();

        var headers = new[]
        {
            "Student ID", "Admission Number", "First Name", "Last Name", "Date of Birth", "Gender",
            "Student Mobile", "Student Email", "Mother Name", "Father Name", "Legacy Parent Name",
            "Primary Parent Mobile", "Secondary Parent Mobile", "Parent Recovery Email", "Current School",
            "Address", "City", "State", "PIN Code", "Admission Date", "Status", "Academic Session",
            "Classes", "Created At"
        };
        var rows = students.Select(s => (IReadOnlyList<string?>)new string?[]
        {
            s.Id.ToString(),
            s.AdmissionNumber,
            s.FirstName,
            s.LastName,
            s.DateOfBirth.ToString("yyyy-MM-dd"),
            s.Gender.ToString(),
            s.StudentMobile,
            s.StudentEmail,
            s.MotherName,
            s.FatherName,
            s.ParentName,
            s.ParentMobile,
            s.SecondaryParentMobile,
            s.ParentEmail,
            s.CurrentSchool,
            s.Address,
            s.City,
            s.State,
            s.PinCode,
            s.AdmissionDate.ToString("yyyy-MM-dd"),
            s.IsActive ? "Active" : "Inactive",
            s.Session?.Name,
            string.Join(", ", s.StudentClasses.Where(sc => sc.IsActive).Select(sc => sc.Class.Name).Distinct()),
            s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });

        var workbook = SimpleExcelWorkbook.Create(headers, rows);
        var fileName = $"MasterMind-All-Students-{DateTime.Today:yyyy-MM-dd}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

        var parentMobileError = NormalizeAndValidateParentMobiles(student);
        if (parentMobileError != null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = parentMobileError
            });
        }

        if (!IsValidDateOfBirth(student.DateOfBirth))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "A valid student date of birth is required"
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
        try
        {
            await LinkParentUserAsync(student);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }

        return CreatedAtAction(nameof(GetStudent),
            new { id = student.Id },
            new ApiResponse<Student>
            {
                Success = true,
                Message = "Student created successfully. Use Copy invite link to share parent access.",
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

        var parentMobileError = NormalizeAndValidateParentMobiles(student);
        if (parentMobileError != null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = parentMobileError
            });
        }

        if (!IsValidDateOfBirth(student.DateOfBirth))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "A valid student date of birth is required"
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
        var becameInactive = existingStudent.IsActive && !student.IsActive;
        var becameActive = !existingStudent.IsActive && student.IsActive;
        existingStudent.IsActive = student.IsActive;
        if (becameInactive)
        {
            existingStudent.InactiveDate = DateTime.Today;
        }
        else if (becameActive)
        {
            existingStudent.InactiveDate = null;
        }
        existingStudent.ParentName = student.ParentName;
        existingStudent.MotherName = student.MotherName;
        existingStudent.FatherName = student.FatherName;
        existingStudent.CurrentSchool = student.CurrentSchool;
        existingStudent.ParentMobile = student.ParentMobile;
        existingStudent.SecondaryParentMobile = student.SecondaryParentMobile;
        existingStudent.ParentEmail = student.ParentEmail;
        existingStudent.ParentOccupation = student.ParentOccupation;
        existingStudent.UpdatedAt = DateTime.UtcNow;

        if (becameInactive)
        {
            var inactiveDate = DateOnly.FromDateTime(existingStudent.InactiveDate!.Value);
            var recurringFees = await _context.StudentFees
                .Where(sf => sf.StudentId == id && !sf.IsDeleted && sf.IsRecurring)
                .ToListAsync();
            foreach (var recurringFee in recurringFees)
            {
                if (!recurringFee.EndDate.HasValue || recurringFee.EndDate.Value > inactiveDate)
                {
                    recurringFee.EndDate = inactiveDate;
                    recurringFee.UpdatedAt = DateTime.UtcNow;
                }
            }

            var unpaidFutureFees = await _context.StudentFees
                .Where(sf => sf.StudentId == id && !sf.IsDeleted && !sf.IsRecurring &&
                    sf.DueDate > inactiveDate && !sf.Payments.Any(p => !p.IsDeleted))
                .ToListAsync();
            foreach (var futureFee in unpaidFutureFees)
            {
                futureFee.IsDeleted = true;
                futureFee.UpdatedAt = DateTime.UtcNow;
            }
        }

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

        try
        {
            await LinkParentUserAsync(student);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        var parentUser = student.ParentUserId.HasValue
            ? await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == student.ParentUserId.Value && !u.IsDeleted)
            : null;
        if (!string.IsNullOrWhiteSpace(parentUser?.PasswordHash))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "This parent has already created a password. They can sign in with their primary mobile number."
            });
        }
        var invitation = await CreateParentInvitationAsync(student);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = invitation.EmailSent
                ? "A fresh invite link was created for WhatsApp and also emailed."
                : "A fresh invite link was created for the primary parent WhatsApp number.",
            Data = new
            {
                invitation.InviteUrl,
                invitation.ExpiresAt,
                invitation.EmailSent,
                PrimaryMobile = student.ParentMobile
            }
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
        var normalizedMobile = NormalizeMobile(student.ParentMobile);
        if (string.IsNullOrWhiteSpace(normalizedMobile))
        {
            return;
        }

        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
        var user = users
            .Where(u => !string.IsNullOrWhiteSpace(u.Mobile) && NormalizeMobile(u.Mobile) == normalizedMobile)
            .OrderByDescending(u => u.UpdatedAt ?? u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .FirstOrDefault();

        if (user == null)
        {
            var displayName = !string.IsNullOrWhiteSpace(student.ParentName)
                ? student.ParentName.Trim()
                : student.MotherName?.Trim() ?? student.FatherName?.Trim() ?? "Parent";
            var nameParts = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user = new User
            {
                Email = BuildParentPlaceholderEmail(normalizedMobile),
                Mobile = normalizedMobile,
                SecondaryMobile = student.SecondaryParentMobile,
                FirstName = nameParts.FirstOrDefault() ?? "Parent",
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                IsActive = true,
                IsEmailVerified = false,
                IsMobileVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (user.UserRoles.Any(ur => ur.Role.Name == "Admin" || ur.Role.Name == "Teacher"))
            {
                throw new InvalidOperationException(
                    "This mobile number belongs to an Admin or Teacher account. Use a separate primary parent mobile number.");
            }
            user.Mobile = normalizedMobile;
            user.SecondaryMobile = student.SecondaryParentMobile;
            user.UpdatedAt = DateTime.UtcNow;
        }

        var siblings = (await _context.Students
                .Where(s => !s.IsDeleted)
                .ToListAsync())
            .Where(s => NormalizeMobile(s.ParentMobile) == normalizedMobile)
            .ToList();
        foreach (var sibling in siblings)
        {
            sibling.ParentUserId = user.Id;
            sibling.SecondaryParentMobile ??= student.SecondaryParentMobile;
            if (!IsPlaceholderEmail(user.Email))
            {
                sibling.ParentEmail = user.Email;
            }
            sibling.UpdatedAt = DateTime.UtcNow;
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

    private async Task<ParentInvitationResult> CreateParentInvitationAsync(Student student)
    {
        if (!student.ParentUserId.HasValue)
        {
            throw new InvalidOperationException("The parent account could not be linked to this student.");
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
        var expiresAt = now.AddHours(72);
        _context.AccountInvitations.Add(new AccountInvitation
        {
            UserId = student.ParentUserId.Value,
            StudentId = student.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
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
        var emailSent = false;
        if (!string.IsNullOrWhiteSpace(student.ParentEmail) && !IsPlaceholderEmail(student.ParentEmail))
        {
            try
            {
                emailSent = await _emailService.SendEmailAsync(
                    student.ParentEmail.Trim(),
                    "Set your MasterMind Coaching parent password",
                    body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Parent invitation {InvitationUserId} was created, but email delivery failed",
                    student.ParentUserId.Value);
            }
        }

        return new ParentInvitationResult(inviteUrl, expiresAt, emailSent);
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

    private static string? NormalizeAndValidateParentMobiles(Student student)
    {
        var primary = NormalizeMobile(student.ParentMobile);
        if (primary.Length != 10)
        {
            return "A valid 10-digit primary parent mobile number is required";
        }

        student.ParentMobile = primary;
        var secondary = NormalizeMobile(student.SecondaryParentMobile);
        if (!string.IsNullOrWhiteSpace(secondary) && secondary.Length != 10)
        {
            return "Secondary parent mobile must be a valid 10-digit number";
        }

        if (secondary == primary)
        {
            return "Secondary parent mobile must be different from the primary mobile";
        }

        student.SecondaryParentMobile = string.IsNullOrWhiteSpace(secondary) ? null : secondary;
        return null;
    }

    private static bool IsValidDateOfBirth(DateTime dateOfBirth) =>
        dateOfBirth.Date >= new DateTime(1900, 1, 1) &&
        dateOfBirth.Date <= DateTime.Today;

    private static string BuildParentPlaceholderEmail(string mobile) =>
        $"parent_{mobile}@placeholder.mastermind.local";

    private static bool IsPlaceholderEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ||
        email.EndsWith("@placeholder.mastermind.local", StringComparison.OrdinalIgnoreCase);

    private sealed record ParentInvitationResult(string InviteUrl, DateTime ExpiresAt, bool EmailSent);
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

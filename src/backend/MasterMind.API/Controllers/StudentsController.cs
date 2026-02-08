using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(MasterMindDbContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all students with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of students</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<Student>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<Student>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? classId = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // Start with a simple query first
            var query = _context.Students.AsQueryable();

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
    /// <returns>Created student</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Student>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Student>>> CreateStudent([FromBody] Student student)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid student data"
            });
        }

        student.CreatedAt = DateTime.UtcNow;
        student.IsActive = true;
        student.IsDeleted = false;

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStudent),
            new { id = student.Id },
            new ApiResponse<Student>
            {
                Success = true,
                Message = "Student created successfully",
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
        existingStudent.AdmissionNumber = student.AdmissionNumber;
        existingStudent.AdmissionDate = student.AdmissionDate;
        existingStudent.IsActive = student.IsActive;
        existingStudent.ParentName = student.ParentName;
        existingStudent.ParentMobile = student.ParentMobile;
        existingStudent.ParentEmail = student.ParentEmail;
        existingStudent.ParentOccupation = student.ParentOccupation;
        existingStudent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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
                // Get students who are not already mapped to this specific class
                query = query.Where(s => !_context.StudentClasses
                    .Any(sc => sc.StudentId == s.Id && sc.ClassId == classId.Value && sc.IsActive));
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

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Student deleted successfully"
        });
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

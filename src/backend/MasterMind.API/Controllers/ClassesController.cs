using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public ClassesController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Classes
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetClasses()
    {
        try
        {
            var classes = await _context.Classes
                .Where(c => c.IsActive && !c.IsDeleted)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .Include(c => c.TeacherClasses)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.StudentClasses)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var classDtos = classes.Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                Medium = c.Medium,
                Board = c.Board,
                AcademicYear = c.AcademicYear,
                Description = c.Description,
                StudentCount = c.StudentClasses.Count(sc => sc.IsActive),
                MaxStudents = c.MaxStudents,
                StartTime = c.StartTime,
                EndTime = c.EndTime,
                DaysOfWeek = c.DaysOfWeek,
                MonthlyFee = c.MonthlyFee,
                IsActive = c.IsActive,
                Subjects = c.ClassSubjects?.Select(cs => cs.Subject?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>(),
                Teachers = c.TeacherClasses?.Where(tc => tc.IsActive && tc.Teacher != null && tc.Teacher.User != null)
                                           .Select(tc => $"{tc.Teacher!.User!.FirstName} {tc.Teacher.User.LastName}")
                                           .ToList() ?? new List<string>(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(new ApiResponse<IEnumerable<ClassDto>>
            {
                Success = true,
                Message = "Classes retrieved successfully",
                Data = classDtos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<ClassDto>>
            {
                Success = false,
                Message = "Error retrieving classes: " + ex.Message,
                Data = null
            });
        }
    }

    // GET: api/Classes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ClassDto>>> GetClass(int id)
    {
        try
        {
            var classEntity = await _context.Classes
                .Where(c => c.Id == id && c.IsActive)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .Include(c => c.TeacherClasses)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.StudentClasses)
                .FirstOrDefaultAsync();

            if (classEntity == null)
            {
                return NotFound(new ApiResponse<ClassDto>
                {
                    Success = false,
                    Message = "Class not found",
                    Data = null
                });
            }

            var classDto = new ClassDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                Section = classEntity.Section,
                Medium = classEntity.Medium,
                Board = classEntity.Board,
                AcademicYear = classEntity.AcademicYear,
                Description = classEntity.Description,
                StudentCount = classEntity.StudentClasses.Count(sc => sc.IsActive),
                MaxStudents = classEntity.MaxStudents,
                StartTime = classEntity.StartTime,
                EndTime = classEntity.EndTime,
                DaysOfWeek = classEntity.DaysOfWeek,
                MonthlyFee = classEntity.MonthlyFee,
                IsActive = classEntity.IsActive,
                Subjects = classEntity.ClassSubjects
                    .Where(cs => cs.IsActive && cs.Subject.IsActive)
                    .Select(cs => cs.Subject.Name)
                    .ToList(),
                Teachers = classEntity.TeacherClasses
                    .Where(tc => tc.IsActive && tc.Teacher != null && tc.Teacher.User != null)
                    .Select(tc => $"{tc.Teacher.User.FirstName} {tc.Teacher.User.LastName}")
                    .ToList(),
                CreatedAt = classEntity.CreatedAt,
                UpdatedAt = classEntity.UpdatedAt
            };

            return Ok(new ApiResponse<ClassDto>
            {
                Success = true,
                Message = "Class retrieved successfully",
                Data = classDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<ClassDto>
            {
                Success = false,
                Message = $"Error retrieving class: {ex.Message}",
                Data = null
            });
        }
    }

    // POST: api/Classes
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClassDto>>> CreateClass(CreateClassDto createClassDto)
    {
        try
        {
            // Get current active session
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);

            if (activeSession == null)
            {
                return BadRequest(new ApiResponse<ClassDto>
                {
                    Success = false,
                    Message = "No active session found. Please activate a session first.",
                    Data = null
                });
            }

            var classEntity = new Class
            {
                Name = createClassDto.Name.Trim(),
                Section = createClassDto.Section?.Trim(),
                Medium = createClassDto.Medium.Trim(),
                Board = createClassDto.Board.Trim(),
                AcademicYear = createClassDto.AcademicYear ?? activeSession.AcademicYear, // Use provided or active session's academic year
                Description = createClassDto.Description?.Trim(),
                MaxStudents = createClassDto.MaxStudents,
                StartTime = createClassDto.StartTime,
                EndTime = createClassDto.EndTime,
                DaysOfWeek = createClassDto.DaysOfWeek?.Trim(),
                MonthlyFee = createClassDto.MonthlyFee,
                IsActive = createClassDto.IsActive ?? true,
                SessionId = activeSession.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            // Add subjects to class
            if (createClassDto.Subjects != null && createClassDto.Subjects.Count > 0)
            {
                await AddSubjectsToClass(classEntity.Id, createClassDto.Subjects);
            }

            // Reload with subjects
            var createdClass = await _context.Classes
                .Where(c => c.Id == classEntity.Id)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .Include(c => c.TeacherClasses)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.StudentClasses)
                .FirstAsync();

            var classDto = MapToClassDto(createdClass);

            return CreatedAtAction(nameof(GetClass), new { id = classDto.Id }, new ApiResponse<ClassDto>
            {
                Success = true,
                Message = "Class created successfully",
                Data = classDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<ClassDto>
            {
                Success = false,
                Message = $"Error creating class: {ex.Message + ex.InnerException?.Message}",
                Data = null
            });
        }
    }

    // PUT: api/Classes/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ClassDto>>> UpdateClass(int id, UpdateClassDto updateClassDto)
    {
        try
        {
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null || !classEntity.IsActive)
            {
                return NotFound(new ApiResponse<ClassDto>
                {
                    Success = false,
                    Message = "Class not found",
                    Data = null
                });
            }

            classEntity.Name = updateClassDto.Name.Trim();
            classEntity.Section = updateClassDto.Section?.Trim();
            if (!string.IsNullOrEmpty(updateClassDto.Medium)) classEntity.Medium = updateClassDto.Medium.Trim();
            if (!string.IsNullOrEmpty(updateClassDto.Board)) classEntity.Board = updateClassDto.Board.Trim();
            classEntity.AcademicYear = updateClassDto.AcademicYear;
            classEntity.Description = updateClassDto.Description?.Trim();
            classEntity.MaxStudents = updateClassDto.MaxStudents;
            classEntity.StartTime = updateClassDto.StartTime;
            classEntity.EndTime = updateClassDto.EndTime;
            classEntity.DaysOfWeek = updateClassDto.DaysOfWeek?.Trim();
            classEntity.MonthlyFee = updateClassDto.MonthlyFee;
            if (updateClassDto.IsActive.HasValue)
            {
                classEntity.IsActive = updateClassDto.IsActive.Value;
            }
            classEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update subjects if provided
            if (updateClassDto.Subjects != null)
            {
                await UpdateClassSubjects(id, updateClassDto.Subjects);
            }

            // Reload with subjects
            var updatedClass = await _context.Classes
                .Where(c => c.Id == id)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .Include(c => c.TeacherClasses)
                    .ThenInclude(tc => tc.Teacher)
                        .ThenInclude(t => t.User)
                .Include(c => c.StudentClasses)
                .FirstAsync();

            var classDto = MapToClassDto(updatedClass);

            return Ok(new ApiResponse<ClassDto>
            {
                Success = true,
                Message = "Class updated successfully",
                Data = classDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<ClassDto>
            {
                Success = false,
                Message = $"Error updating class: {ex.Message + ex.InnerException?.Message}",
                Data = null
            });
        }
    }

    // DELETE: api/Classes/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteClass(int id)
    {
        try
        {
            var classEntity = await _context.Classes.FindAsync(id);
            if (classEntity == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Class not found",
                    Data = false
                });
            }

            // Soft delete
            classEntity.IsActive = false;
            classEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Class deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting class: {ex.Message}",
                Data = false
            });
        }
    }

    // Helper methods
    private async Task AddSubjectsToClass(int classId, List<string> subjectNames)
    {
        try
        {
        foreach (var subjectName in subjectNames)
        {
            var trimmedName = subjectName.Trim();
            if (string.IsNullOrEmpty(trimmedName)) continue;

            // Find or create subject
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.ToLower() == trimmedName.ToLower());

            if (subject == null)
            {
                subject = new Subject
                {
                    Name = trimmedName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }

            // Check if class-subject relationship already exists
            var existingClassSubject = await _context.ClassSubjects
                .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.SubjectId == subject.Id);

            if (existingClassSubject == null)
            {
                var classSubject = new ClassSubject
                {
                    ClassId = classId,
                    SubjectId = subject.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                // Do not set the Id explicitly as it is an identity column
                _context.ClassSubjects.Add(classSubject);
            }
        }

        await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding subjects to class: " + ex.Message + ex.InnerException?.Message);
        }
    }

    private async Task UpdateClassSubjects(int classId, List<string> subjectNames)
    {
        // Remove existing class-subject relationships
        var existingClassSubjects = await _context.ClassSubjects
            .Where(cs => cs.ClassId == classId)
            .ToListAsync();

        foreach (var cs in existingClassSubjects)
        {
            cs.IsActive = false;
            cs.UpdatedAt = DateTime.UtcNow;
        }

        // Add new subjects
        await AddSubjectsToClass(classId, subjectNames);
    }

    private ClassDto MapToClassDto(Class classEntity)
    {
        return new ClassDto
        {
            Id = classEntity.Id,
            Name = classEntity.Name,
            Section = classEntity.Section,
            Medium = classEntity.Medium,
            Board = classEntity.Board,
            AcademicYear = classEntity.AcademicYear,
            Description = classEntity.Description,
            StudentCount = classEntity.StudentClasses?.Count(sc => sc.IsActive) ?? 0,
            MaxStudents = classEntity.MaxStudents,
            StartTime = classEntity.StartTime,
            EndTime = classEntity.EndTime,
            DaysOfWeek = classEntity.DaysOfWeek,
            MonthlyFee = classEntity.MonthlyFee,
            IsActive = classEntity.IsActive,
            Subjects = classEntity.ClassSubjects
                .Where(cs => cs.IsActive && cs.Subject.IsActive)
                .Select(cs => cs.Subject.Name)
                .ToList(),
            Teachers = classEntity.TeacherClasses?
                .Where(tc => tc.IsActive && tc.Teacher != null && tc.Teacher.User != null)
                .Select(tc => $"{tc.Teacher!.User!.FirstName} {tc.Teacher.User.LastName}")
                .ToList() ?? new List<string>(),
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }
}

// DTOs
public class ClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string Medium { get; set; } = string.Empty;
    public string Board { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StudentCount { get; set; }
    public int? MaxStudents { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public decimal? MonthlyFee { get; set; }
    public bool IsActive { get; set; }
    public List<string> Subjects { get; set; } = new List<string>();
    public List<string> Teachers { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string Medium { get; set; } = "English";
    public string Board { get; set; } = "CBSE";
    public string? AcademicYear { get; set; } // Optional, will be set from active session
    public string? Description { get; set; }
    public int? MaxStudents { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public decimal? MonthlyFee { get; set; }
    public List<string>? Subjects { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Medium { get; set; }
    public string? Board { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxStudents { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public decimal? MonthlyFee { get; set; }
    public List<string>? Subjects { get; set; }
    public bool? IsActive { get; set; }
}
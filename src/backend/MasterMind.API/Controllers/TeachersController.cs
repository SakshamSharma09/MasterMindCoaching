using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;
using System.Text.Json;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public TeachersController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Teachers
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Teacher>>>> GetTeachers()
    {
        try
        {
            var teachers = await _context.Teachers
                .Where(t => !t.IsDeleted)
                .Include(t => t.TeacherClasses)
                    .ThenInclude(tc => tc.Class)
                .Select(t => new Teacher
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    Mobile = t.Mobile,
                    Specialization = t.Specialization,
                    Qualification = t.Qualification,
                    ExperienceYears = t.ExperienceYears,
                    MonthlySalary = t.MonthlySalary,
                    JoiningDate = t.JoiningDate,
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
                    Specialization = t.Specialization,
                    Qualification = t.Qualification,
                    ExperienceYears = t.ExperienceYears,
                    MonthlySalary = t.MonthlySalary,
                    JoiningDate = t.JoiningDate,
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

            // Create new teacher entity
            var teacher = new Teacher
            {
                FirstName = teacherData.GetProperty("firstName").GetString(),
                LastName = teacherData.GetProperty("lastName").GetString(),
                Email = teacherData.GetProperty("email").GetString(),
                Mobile = teacherData.GetProperty("mobile").GetString(),
                Specialization = teacherData.GetProperty("specialization").GetString(),
                Qualification = teacherData.GetProperty("qualification").GetString(),
                
                // Handle Subjects property if it exists
                Subjects = teacherData.TryGetProperty("subjects", out JsonElement subjectsElement) ? subjectsElement.GetString() : null,
                
                ExperienceYears = teacherData.GetProperty("experienceYears").GetInt32(),
                MonthlySalary = teacherData.GetProperty("monthlySalary").GetDecimal(),
                JoiningDate = DateTime.Parse(teacherData.GetProperty("joiningDate").GetString()),
                IsActive = teacherData.GetProperty("isActive").GetBoolean(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SessionId = activeSession.Id
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

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

            // Update basic teacher properties
            teacher.FirstName = updateData.GetProperty("firstName").GetString();
            teacher.LastName = updateData.GetProperty("lastName").GetString();
            teacher.Email = updateData.GetProperty("email").GetString();
            teacher.Mobile = updateData.GetProperty("mobile").GetString();
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
            teacher.UpdatedAt = DateTime.UtcNow;

            // Handle class assignments if provided
            if (updateData.TryGetProperty("classIds", out JsonElement classIdsElement))
            {
                // Remove existing class assignments
                var existingClasses = _context.TeacherClasses.Where(tc => tc.TeacherId == id).ToList();
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
}
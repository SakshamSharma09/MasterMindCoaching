using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public SubjectsController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Subjects
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Subject>>>> GetSubjects()
    {
        try
        {
            var subjects = await _context.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Subject>>
            {
                Success = true,
                Message = "Subjects retrieved successfully",
                Data = subjects
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Subject>>
            {
                Success = false,
                Message = $"Error retrieving subjects: {ex.Message}",
                Data = null
            });
        }
    }

    // GET: api/Subjects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Subject>>> GetSubject(int id)
    {
        try
        {
            var subject = await _context.Subjects
                .Where(s => s.Id == id && s.IsActive)
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                return NotFound(new ApiResponse<Subject>
                {
                    Success = false,
                    Message = "Subject not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Subject>
            {
                Success = true,
                Message = "Subject retrieved successfully",
                Data = subject
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Subject>
            {
                Success = false,
                Message = $"Error retrieving subject: {ex.Message}",
                Data = null
            });
        }
    }

    // POST: api/Subjects
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Subject>>> CreateSubject(CreateSubjectDto createSubjectDto)
    {
        try
        {
            // Check if subject already exists
            var existingSubject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Name.ToLower() == createSubjectDto.Name.ToLower());

            if (existingSubject != null)
            {
                return BadRequest(new ApiResponse<Subject>
                {
                    Success = false,
                    Message = "Subject with this name already exists",
                    Data = null
                });
            }

            var subject = new Subject
            {
                Name = createSubjectDto.Name.Trim(),
                Code = createSubjectDto.Code?.Trim(),
                Description = createSubjectDto.Description?.Trim(),
                Category = createSubjectDto.Category?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, new ApiResponse<Subject>
            {
                Success = true,
                Message = "Subject created successfully",
                Data = subject
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Subject>
            {
                Success = false,
                Message = $"Error creating subject: {ex.Message}",
                Data = null
            });
        }
    }

    // PUT: api/Subjects/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Subject>>> UpdateSubject(int id, UpdateSubjectDto updateSubjectDto)
    {
        try
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null || !subject.IsActive)
            {
                return NotFound(new ApiResponse<Subject>
                {
                    Success = false,
                    Message = "Subject not found",
                    Data = null
                });
            }

            // Check if name is being changed and if it conflicts with existing subject
            if (updateSubjectDto.Name.ToLower() != subject.Name.ToLower())
            {
                var existingSubject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == updateSubjectDto.Name.ToLower() && s.Id != id);

                if (existingSubject != null)
                {
                    return BadRequest(new ApiResponse<Subject>
                    {
                        Success = false,
                        Message = "Subject with this name already exists",
                        Data = null
                    });
                }
            }

            subject.Name = updateSubjectDto.Name.Trim();
            subject.Code = updateSubjectDto.Code?.Trim();
            subject.Description = updateSubjectDto.Description?.Trim();
            subject.Category = updateSubjectDto.Category?.Trim();
            subject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Subject>
            {
                Success = true,
                Message = "Subject updated successfully",
                Data = subject
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Subject>
            {
                Success = false,
                Message = $"Error updating subject: {ex.Message}",
                Data = null
            });
        }
    }

    // DELETE: api/Subjects/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubject(int id)
    {
        try
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Subject not found",
                    Data = false
                });
            }

            // Soft delete
            subject.IsActive = false;
            subject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Subject deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting subject: {ex.Message}",
                Data = false
            });
        }
    }

    // GET: api/Subjects/suggestions
    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSubjectSuggestions([FromQuery] string query = "")
    {
        try
        {
            var suggestions = await _context.Subjects
                .Where(s => s.IsActive && (string.IsNullOrEmpty(query) || s.Name.ToLower().Contains(query.ToLower())))
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .Take(10)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Message = "Subject suggestions retrieved successfully",
                Data = suggestions
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = $"Error retrieving subject suggestions: {ex.Message}",
                Data = new List<string>()
            });
        }
    }

    // GET: api/Subjects/by-class/5
    [HttpGet("by-class/{classId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Subject>>>> GetSubjectsByClass(int classId)
    {
        try
        {
            var subjects = await _context.ClassSubjects
                .Where(cs => cs.ClassId == classId && cs.IsActive)
                .Include(cs => cs.Subject)
                .Where(cs => cs.Subject.IsActive)
                .Select(cs => cs.Subject)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Subject>>
            {
                Success = true,
                Message = "Subjects for class retrieved successfully",
                Data = subjects
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Subject>>
            {
                Success = false,
                Message = $"Error retrieving subjects for class: {ex.Message}",
                Data = new List<Subject>()
            });
        }
    }
}

// DTOs
public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

public class UpdateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

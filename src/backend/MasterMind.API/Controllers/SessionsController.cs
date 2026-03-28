using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public SessionsController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Sessions
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<Session>>>> GetSessions()
    {
        try
        {
            var sessions = await _context.Sessions
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<Session>>
            {
                Success = true,
                Message = "Sessions retrieved successfully",
                Data = sessions
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<Session>>
            {
                Success = false,
                Message = "Error retrieving sessions: " + ex.Message,
                Data = null
            });
        }
    }

    // GET: api/Sessions/active
    [HttpGet("active")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<Session>>> GetActiveSession()
    {
        try
        {
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);

            if (activeSession == null)
            {
                return NotFound(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "No active session found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Session>
            {
                Success = true,
                Message = "Active session retrieved successfully",
                Data = activeSession
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Session>
            {
                Success = false,
                Message = "Error retrieving active session: " + ex.Message,
                Data = null
            });
        }
    }

    // POST: api/Sessions
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Session>>> CreateSession(CreateSessionDto createDto)
    {
        try
        {
            // Validate dates
            if (createDto.StartDate >= createDto.EndDate)
            {
                return BadRequest(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "Start date must be before end date",
                    Data = null
                });
            }

            // Check if session name already exists
            var existingSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted);

            if (existingSession != null)
            {
                return BadRequest(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "A session with this name already exists",
                    Data = null
                });
            }

            var session = new Session
            {
                Name = createDto.Name,
                DisplayName = createDto.DisplayName ?? createDto.Name,
                Description = createDto.Description,
                AcademicYear = createDto.AcademicYear,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Status = createDto.Status,
                IsActive = createDto.IsActive,
                Settings = createDto.Settings,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If this is the first session, make it active
            var hasAnyActiveSession = await _context.Sessions.AnyAsync(s => s.IsActive && !s.IsDeleted);
            if (!hasAnyActiveSession)
            {
                session.IsActive = true;
                session.Status = SessionStatus.Active;
            }

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSessions), new { id = session.Id }, new ApiResponse<Session>
            {
                Success = true,
                Message = "Session created successfully",
                Data = session
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<Session>
            {
                Success = false,
                Message = "Error creating session: " + ex.Message,
                Data = null
            });
        }
    }

    // PUT: api/Sessions/5/activate
    [HttpPut("{id}/activate")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateSession(int id)
    {
        try
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null || session.IsDeleted)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Session not found",
                    Data = false
                });
            }

            // Deactivate all other sessions
            var otherActiveSessions = await _context.Sessions
                .Where(s => s.IsActive && s.Id != id && !s.IsDeleted)
                .ToListAsync();

            foreach (var otherSession in otherActiveSessions)
            {
                otherSession.IsActive = false;
                otherSession.UpdatedAt = DateTime.UtcNow;
            }

            // Activate this session
            session.IsActive = true;
            session.Status = SessionStatus.Active;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Session activated successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error activating session: " + ex.Message,
                Data = false
            });
        }
    }
}

// DTOs
public class CreateSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Planned;
    public bool IsActive { get; set; } = false;
    public string? Settings { get; set; }
}

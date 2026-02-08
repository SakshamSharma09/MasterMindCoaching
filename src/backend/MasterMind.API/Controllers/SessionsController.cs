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

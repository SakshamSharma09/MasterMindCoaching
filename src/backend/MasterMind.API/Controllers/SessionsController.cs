using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionsController> _logger;
    private const string SessionsCacheKey = "all_sessions";

    public SessionsController(MasterMindDbContext context, IMemoryCache cache, ILogger<SessionsController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    // GET: api/Sessions
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Session>>>> GetSessions()
    {
        try
        {
            var sessions = await _cache.GetOrCreateAsync(SessionsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _context.Sessions
                    .OrderByDescending(s => s.StartDate)
                    .ToListAsync();
            });

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

    // GET: api/Sessions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Session>>> GetSession(int id)
    {
        try
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (session == null)
            {
                return NotFound(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "Session not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<Session>
            {
                Success = true,
                Message = "Session retrieved successfully",
                Data = session
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", id);
            return StatusCode(500, new ApiResponse<Session>
            {
                Success = false,
                Message = "Error retrieving session",
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

    // POST: api/Sessions
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Session>>> CreateSession([FromBody] CreateSessionDto createDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return BadRequest(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "Session name is required",
                    Data = null
                });
            }

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

            // Check if session name already exists - load data first to avoid SQL translation issues
            var existingSessions = await _context.Sessions
                .Where(s => !s.IsDeleted)
                .ToListAsync();
            
            var existingSession = existingSessions
                .FirstOrDefault(s => s.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase));

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
                AcademicYear = createDto.AcademicYear ?? GetCurrentAcademicYear(),
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Status = createDto.GetStatus(),
                IsActive = createDto.IsActive,
                Settings = createDto.Settings,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If this is the first session or no active session exists, make it active
            var hasAnyActiveSession = await _context.Sessions.AnyAsync(s => s.IsActive && !s.IsDeleted);
            if (!hasAnyActiveSession)
            {
                session.IsActive = true;
                session.Status = SessionStatus.Active;
            }

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            _cache.Remove(SessionsCacheKey);

            _logger.LogInformation("Session created: {SessionName} (ID: {SessionId})", session.Name, session.Id);

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, new ApiResponse<Session>
            {
                Success = true,
                Message = "Session created successfully",
                Data = session
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, new ApiResponse<Session>
            {
                Success = false,
                Message = "Error creating session: " + ex.Message,
                Data = null
            });
        }
    }

    // PUT: api/Sessions/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Session>>> UpdateSession(int id, [FromBody] UpdateSessionDto updateDto)
    {
        try
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (session == null)
            {
                return NotFound(new ApiResponse<Session>
                {
                    Success = false,
                    Message = "Session not found",
                    Data = null
                });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDto.Name)) session.Name = updateDto.Name;
            if (!string.IsNullOrEmpty(updateDto.DisplayName)) session.DisplayName = updateDto.DisplayName;
            if (updateDto.Description != null) session.Description = updateDto.Description;
            if (!string.IsNullOrEmpty(updateDto.AcademicYear)) session.AcademicYear = updateDto.AcademicYear;
            if (updateDto.StartDate.HasValue) session.StartDate = updateDto.StartDate.Value;
            if (updateDto.EndDate.HasValue) session.EndDate = updateDto.EndDate.Value;
            var parsedStatus = updateDto.GetStatus();
            if (parsedStatus.HasValue) session.Status = parsedStatus.Value;
            if (updateDto.Settings != null) session.Settings = updateDto.Settings;

            // Handle IsActive change - if activating this session, deactivate others
            if (updateDto.IsActive.HasValue)
            {
                if (updateDto.IsActive.Value && !session.IsActive)
                {
                    // Deactivate all other sessions
                    var otherActiveSessions = await _context.Sessions
                        .Where(s => s.IsActive && s.Id != id && !s.IsDeleted)
                        .ToListAsync();

                    foreach (var otherSession in otherActiveSessions)
                    {
                        otherSession.IsActive = false;
                        otherSession.UpdatedAt = DateTime.UtcNow;
                    }

                    session.Status = SessionStatus.Active;
                }
                session.IsActive = updateDto.IsActive.Value;
            }

            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove(SessionsCacheKey);

            _logger.LogInformation("Session updated: {SessionName} (ID: {SessionId})", session.Name, session.Id);

            return Ok(new ApiResponse<Session>
            {
                Success = true,
                Message = "Session updated successfully",
                Data = session
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", id);
            return StatusCode(500, new ApiResponse<Session>
            {
                Success = false,
                Message = "Error updating session: " + ex.Message,
                Data = null
            });
        }
    }

    // DELETE: api/Sessions/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSession(int id)
    {
        try
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (session == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Session not found",
                    Data = false
                });
            }

            // Don't allow deleting active session
            if (session.IsActive)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete an active session. Please activate another session first.",
                    Data = false
                });
            }

            session.IsDeleted = true;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove(SessionsCacheKey);

            _logger.LogInformation("Session deleted: {SessionName} (ID: {SessionId})", session.Name, session.Id);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Session deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting session: " + ex.Message,
                Data = false
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
            _cache.Remove(SessionsCacheKey);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Session activated successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating session {SessionId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error activating session: " + ex.Message,
                Data = false
            });
        }
    }

    #region Helper Methods

    private static string GetCurrentAcademicYear()
    {
        var now = DateTime.Now;
        var year = now.Month >= 4 ? now.Year : now.Year - 1;
        return $"{year}-{(year + 1) % 100:D2}";
    }

    #endregion
}

// DTOs
public class CreateSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? AcademicYear { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; } = false;
    public string? Settings { get; set; }

    public SessionStatus GetStatus()
    {
        if (string.IsNullOrEmpty(Status)) return SessionStatus.Planned;
        return Status.ToLower() switch
        {
            "planned" => SessionStatus.Planned,
            "active" => SessionStatus.Active,
            "completed" => SessionStatus.Completed,
            "suspended" => SessionStatus.Suspended,
            "cancelled" => SessionStatus.Cancelled,
            _ => SessionStatus.Planned
        };
    }
}

public class UpdateSessionDto
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? AcademicYear { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public string? Settings { get; set; }

    public SessionStatus? GetStatus()
    {
        if (string.IsNullOrEmpty(Status)) return null;
        return Status.ToLower() switch
        {
            "planned" => SessionStatus.Planned,
            "active" => SessionStatus.Active,
            "completed" => SessionStatus.Completed,
            "suspended" => SessionStatus.Suspended,
            "cancelled" => SessionStatus.Cancelled,
            _ => null
        };
    }
}

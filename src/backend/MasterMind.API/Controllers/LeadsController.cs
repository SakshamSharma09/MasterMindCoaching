using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(MasterMindDbContext context, ILogger<LeadsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all leads with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeadDto>>>> GetLeads(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? source,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Leads
                .Include(l => l.AssignedToUser)
                .Include(l => l.LeadFollowups)
                .Where(l => !l.IsDeleted)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(l => 
                    l.Name.ToLower().Contains(searchLower) ||
                    l.Mobile.Contains(search) ||
                    (l.Email != null && l.Email.ToLower().Contains(searchLower)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, true, out var leadStatus))
            {
                query = query.Where(l => l.Status == leadStatus);
            }

            // Apply source filter
            if (!string.IsNullOrEmpty(source) && Enum.TryParse<LeadSource>(source, true, out var leadSource))
            {
                query = query.Where(l => l.Source == leadSource);
            }

            var totalCount = await query.CountAsync();

            var leads = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var leadDtos = leads.Select(MapToDto).ToList();

            return Ok(new ApiResponse<IEnumerable<LeadDto>>
            {
                Success = true,
                Message = $"Retrieved {leadDtos.Count} leads",
                Data = leadDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads");
            return StatusCode(500, new ApiResponse<IEnumerable<LeadDto>>
            {
                Success = false,
                Message = "Error retrieving leads"
            });
        }
    }

    /// <summary>
    /// Get a specific lead by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> GetLead(int id)
    {
        try
        {
            var lead = await _context.Leads
                .Include(l => l.AssignedToUser)
                .Include(l => l.LeadFollowups)
                    .ThenInclude(f => f.FollowedByUser)
                .Include(l => l.ConvertedStudent)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (lead == null)
            {
                return NotFound(new ApiResponse<LeadDto>
                {
                    Success = false,
                    Message = "Lead not found"
                });
            }

            return Ok(new ApiResponse<LeadDto>
            {
                Success = true,
                Message = "Lead retrieved successfully",
                Data = MapToDto(lead)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead {LeadId}", id);
            return StatusCode(500, new ApiResponse<LeadDto>
            {
                Success = false,
                Message = "Error retrieving lead"
            });
        }
    }

    /// <summary>
    /// Create a new lead
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> CreateLead([FromBody] CreateLeadDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest(new ApiResponse<LeadDto>
                {
                    Success = false,
                    Message = "Name and phone are required"
                });
            }

            // Check for duplicate phone number
            var existingLead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Mobile == dto.Phone && !l.IsDeleted);

            if (existingLead != null)
            {
                return BadRequest(new ApiResponse<LeadDto>
                {
                    Success = false,
                    Message = "A lead with this phone number already exists"
                });
            }

            var lead = new Lead
            {
                Name = dto.Name,
                Mobile = dto.Phone,
                Email = dto.Email,
                ParentName = dto.ParentName,
                ParentMobile = dto.ParentMobile,
                Address = dto.Address,
                City = dto.City,
                Source = ParseLeadSource(dto.Source),
                SourceDetails = dto.SourceDetails,
                InterestedClass = dto.InterestedClass,
                InterestedSubject = dto.InterestedSubject,
                Status = LeadStatus.New,
                Priority = ParseLeadPriority(dto.Priority),
                NextFollowupDate = dto.NextFollowup != null ? DateTime.Parse(dto.NextFollowup) : null,
                Remarks = dto.Notes,
                AssignedToUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            // Reload with includes
            lead = await _context.Leads
                .Include(l => l.AssignedToUser)
                .FirstOrDefaultAsync(l => l.Id == lead.Id);

            return CreatedAtAction(nameof(GetLead), new { id = lead!.Id }, new ApiResponse<LeadDto>
            {
                Success = true,
                Message = "Lead created successfully",
                Data = MapToDto(lead)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lead");
            return StatusCode(500, new ApiResponse<LeadDto>
            {
                Success = false,
                Message = "Error creating lead"
            });
        }
    }

    /// <summary>
    /// Update an existing lead
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> UpdateLead(int id, [FromBody] UpdateLeadDto dto)
    {
        try
        {
            var lead = await _context.Leads
                .Include(l => l.AssignedToUser)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (lead == null)
            {
                return NotFound(new ApiResponse<LeadDto>
                {
                    Success = false,
                    Message = "Lead not found"
                });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Name)) lead.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Phone)) lead.Mobile = dto.Phone;
            if (dto.Email != null) lead.Email = dto.Email;
            if (dto.ParentName != null) lead.ParentName = dto.ParentName;
            if (dto.ParentMobile != null) lead.ParentMobile = dto.ParentMobile;
            if (dto.Address != null) lead.Address = dto.Address;
            if (dto.City != null) lead.City = dto.City;
            if (!string.IsNullOrEmpty(dto.Source)) lead.Source = ParseLeadSource(dto.Source);
            if (!string.IsNullOrEmpty(dto.Status)) lead.Status = ParseLeadStatus(dto.Status);
            if (!string.IsNullOrEmpty(dto.Priority)) lead.Priority = ParseLeadPriority(dto.Priority);
            if (dto.InterestedClass != null) lead.InterestedClass = dto.InterestedClass;
            if (dto.InterestedSubject != null) lead.InterestedSubject = dto.InterestedSubject;
            if (dto.Notes != null) lead.Remarks = dto.Notes;
            if (!string.IsNullOrEmpty(dto.NextFollowup)) lead.NextFollowupDate = DateTime.Parse(dto.NextFollowup);

            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<LeadDto>
            {
                Success = true,
                Message = "Lead updated successfully",
                Data = MapToDto(lead)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead {LeadId}", id);
            return StatusCode(500, new ApiResponse<LeadDto>
            {
                Success = false,
                Message = "Error updating lead"
            });
        }
    }

    /// <summary>
    /// Delete a lead (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLead(int id)
    {
        try
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (lead == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lead not found",
                    Data = false
                });
            }

            lead.IsDeleted = true;
            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Lead deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lead {LeadId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting lead"
            });
        }
    }

    /// <summary>
    /// Add a follow-up to a lead
    /// </summary>
    [HttpPost("{id}/followup")]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeadDto>>> AddFollowup(int id, [FromBody] AddFollowupDto dto)
    {
        try
        {
            var lead = await _context.Leads
                .Include(l => l.LeadFollowups)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (lead == null)
            {
                return NotFound(new ApiResponse<LeadDto>
                {
                    Success = false,
                    Message = "Lead not found"
                });
            }

            var followup = new LeadFollowup
            {
                LeadId = id,
                FollowupDate = DateTime.UtcNow,
                Type = ParseFollowupType(dto.Type),
                Notes = dto.Notes ?? string.Empty,
                StatusAfter = !string.IsNullOrEmpty(dto.Status) ? ParseLeadStatus(dto.Status) : null,
                NextFollowupDate = !string.IsNullOrEmpty(dto.NextFollowup) ? DateTime.Parse(dto.NextFollowup) : null,
                NextFollowupNotes = dto.NextFollowupNotes,
                FollowedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.LeadFollowups.Add(followup);

            // Update lead status if provided
            if (!string.IsNullOrEmpty(dto.Status))
            {
                lead.Status = ParseLeadStatus(dto.Status);
            }

            // Update next followup date on lead
            if (!string.IsNullOrEmpty(dto.NextFollowup))
            {
                lead.NextFollowupDate = DateTime.Parse(dto.NextFollowup);
            }

            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload with all includes
            lead = await _context.Leads
                .Include(l => l.AssignedToUser)
                .Include(l => l.LeadFollowups)
                .FirstOrDefaultAsync(l => l.Id == id);

            return Ok(new ApiResponse<LeadDto>
            {
                Success = true,
                Message = "Follow-up added successfully",
                Data = MapToDto(lead!)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding followup to lead {LeadId}", id);
            return StatusCode(500, new ApiResponse<LeadDto>
            {
                Success = false,
                Message = "Error adding follow-up"
            });
        }
    }

    /// <summary>
    /// Get lead statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<LeadStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeadStatsDto>>> GetLeadStats()
    {
        try
        {
            var leads = await _context.Leads
                .Where(l => !l.IsDeleted)
                .ToListAsync();

            var stats = new LeadStatsDto
            {
                Total = leads.Count,
                New = leads.Count(l => l.Status == LeadStatus.New),
                Contacted = leads.Count(l => l.Status == LeadStatus.Contacted),
                Interested = leads.Count(l => l.Status == LeadStatus.Interested),
                FollowUp = leads.Count(l => l.Status == LeadStatus.FollowUp),
                Negotiation = leads.Count(l => l.Status == LeadStatus.Negotiation),
                Converted = leads.Count(l => l.Status == LeadStatus.Converted),
                Lost = leads.Count(l => l.Status == LeadStatus.Lost || l.Status == LeadStatus.NotInterested)
            };

            return Ok(new ApiResponse<LeadStatsDto>
            {
                Success = true,
                Message = "Lead statistics retrieved successfully",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead statistics");
            return StatusCode(500, new ApiResponse<LeadStatsDto>
            {
                Success = false,
                Message = "Error retrieving lead statistics"
            });
        }
    }

    /// <summary>
    /// Convert a lead to a student
    /// </summary>
    [HttpPost("{id}/convert")]
    [ProducesResponseType(typeof(ApiResponse<ConvertLeadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ConvertLeadResultDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ConvertLeadResultDto>>> ConvertToStudent(int id, [FromBody] ConvertLeadDto dto)
    {
        try
        {
            var lead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            if (lead == null)
            {
                return NotFound(new ApiResponse<ConvertLeadResultDto>
                {
                    Success = false,
                    Message = "Lead not found"
                });
            }

            if (lead.Status == LeadStatus.Converted)
            {
                return BadRequest(new ApiResponse<ConvertLeadResultDto>
                {
                    Success = false,
                    Message = "Lead has already been converted"
                });
            }

            // Get active session
            var activeSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);

            // Create student from lead
            var student = new Student
            {
                FirstName = GetFirstName(lead.Name),
                LastName = GetLastName(lead.Name),
                StudentMobile = lead.Mobile,
                StudentEmail = lead.Email,
                ParentName = lead.ParentName ?? string.Empty,
                ParentMobile = lead.ParentMobile ?? lead.Mobile,
                ParentEmail = lead.Email,
                Address = lead.Address,
                City = lead.City,
                DateOfBirth = dto.DateOfBirth ?? DateTime.Today.AddYears(-15),
                Gender = dto.Gender ?? Gender.Male,
                AdmissionDate = DateTime.UtcNow,
                AdmissionNumber = GenerateAdmissionNumber(),
                IsActive = true,
                SessionId = activeSession?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Map student to class if provided
            if (dto.ClassId.HasValue)
            {
                var studentClass = new StudentClass
                {
                    StudentId = student.Id,
                    ClassId = dto.ClassId.Value,
                    IsActive = true,
                    EnrollmentDate = DateTime.UtcNow
                };
                _context.StudentClasses.Add(studentClass);
            }

            // Update lead status
            lead.Status = LeadStatus.Converted;
            lead.ConvertedStudentId = student.Id;
            lead.ConvertedAt = DateTime.UtcNow;
            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<ConvertLeadResultDto>
            {
                Success = true,
                Message = "Lead converted to student successfully",
                Data = new ConvertLeadResultDto
                {
                    LeadId = lead.Id,
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    AdmissionNumber = student.AdmissionNumber
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting lead {LeadId} to student", id);
            return StatusCode(500, new ApiResponse<ConvertLeadResultDto>
            {
                Success = false,
                Message = "Error converting lead to student"
            });
        }
    }

    #region Helper Methods

    private LeadDto MapToDto(Lead lead)
    {
        var lastFollowup = lead.LeadFollowups?
            .OrderByDescending(f => f.FollowupDate)
            .FirstOrDefault();

        return new LeadDto
        {
            Id = lead.Id,
            Name = lead.Name,
            Phone = lead.Mobile,
            Email = lead.Email ?? string.Empty,
            ParentName = lead.ParentName,
            ParentMobile = lead.ParentMobile,
            Address = lead.Address,
            City = lead.City,
            Source = lead.Source.ToString(),
            SourceDetails = lead.SourceDetails,
            Status = lead.Status.ToString(),
            Priority = lead.Priority.ToString(),
            InterestedClass = lead.InterestedClass,
            InterestedSubject = lead.InterestedSubject,
            NextFollowup = lead.NextFollowupDate?.ToString("yyyy-MM-dd"),
            LastFollowup = lastFollowup?.FollowupDate.ToString("yyyy-MM-dd") ?? lead.CreatedAt.ToString("yyyy-MM-dd"),
            Notes = lead.Remarks,
            AssignedTo = lead.AssignedToUser != null 
                ? $"{lead.AssignedToUser.FirstName} {lead.AssignedToUser.LastName}" 
                : null,
            ConvertedStudentId = lead.ConvertedStudentId,
            ConvertedAt = lead.ConvertedAt?.ToString("yyyy-MM-dd"),
            CreatedAt = lead.CreatedAt.ToString("yyyy-MM-dd"),
            UpdatedAt = lead.UpdatedAt?.ToString("yyyy-MM-dd"),
            FollowupCount = lead.LeadFollowups?.Count ?? 0
        };
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static LeadSource ParseLeadSource(string? source)
    {
        if (string.IsNullOrEmpty(source)) return LeadSource.Other;
        
        return source.ToLower() switch
        {
            "website" => LeadSource.Website,
            "phone" or "phonecall" => LeadSource.PhoneCall,
            "referral" => LeadSource.Referral,
            "social media" or "socialmedia" => LeadSource.SocialMedia,
            "advertisement" => LeadSource.Advertisement,
            "walkin" or "walk-in" => LeadSource.WalkIn,
            "event" => LeadSource.Event,
            _ => LeadSource.Other
        };
    }

    private static LeadStatus ParseLeadStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return LeadStatus.New;
        
        return status.ToLower() switch
        {
            "new" => LeadStatus.New,
            "contacted" => LeadStatus.Contacted,
            "interested" => LeadStatus.Interested,
            "followup" or "follow-up" => LeadStatus.FollowUp,
            "negotiation" => LeadStatus.Negotiation,
            "converted" => LeadStatus.Converted,
            "lost" => LeadStatus.Lost,
            "notinterested" or "not interested" => LeadStatus.NotInterested,
            _ => LeadStatus.New
        };
    }

    private static LeadPriority ParseLeadPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority)) return LeadPriority.Medium;
        
        return priority.ToLower() switch
        {
            "low" => LeadPriority.Low,
            "medium" => LeadPriority.Medium,
            "high" => LeadPriority.High,
            "urgent" => LeadPriority.Urgent,
            _ => LeadPriority.Medium
        };
    }

    private static FollowupType ParseFollowupType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return FollowupType.PhoneCall;
        
        return type.ToLower() switch
        {
            "phonecall" or "phone" or "call" => FollowupType.PhoneCall,
            "email" => FollowupType.Email,
            "sms" => FollowupType.SMS,
            "whatsapp" => FollowupType.WhatsApp,
            "meeting" => FollowupType.Meeting,
            "visit" => FollowupType.Visit,
            _ => FollowupType.Other
        };
    }

    private static string GetFirstName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : fullName;
    }

    private static string GetLastName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
    }

    private string GenerateAdmissionNumber()
    {
        var year = DateTime.Now.Year.ToString().Substring(2);
        var random = new Random().Next(1000, 9999);
        return $"MM{year}{random}";
    }

    #endregion
}

#region DTOs

public class LeadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public string? ParentMobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? SourceDetails { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? InterestedClass { get; set; }
    public string? InterestedSubject { get; set; }
    public string? NextFollowup { get; set; }
    public string LastFollowup { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? AssignedTo { get; set; }
    public int? ConvertedStudentId { get; set; }
    public string? ConvertedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public int FollowupCount { get; set; }
}

public class CreateLeadDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ParentName { get; set; }
    public string? ParentMobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Source { get; set; }
    public string? SourceDetails { get; set; }
    public string? InterestedClass { get; set; }
    public string? InterestedSubject { get; set; }
    public string? Priority { get; set; }
    public string? NextFollowup { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLeadDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ParentName { get; set; }
    public string? ParentMobile { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Source { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? InterestedClass { get; set; }
    public string? InterestedSubject { get; set; }
    public string? NextFollowup { get; set; }
    public string? Notes { get; set; }
}

public class AddFollowupDto
{
    public string? Type { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public string? NextFollowup { get; set; }
    public string? NextFollowupNotes { get; set; }
}

public class LeadStatsDto
{
    public int Total { get; set; }
    public int New { get; set; }
    public int Contacted { get; set; }
    public int Interested { get; set; }
    public int FollowUp { get; set; }
    public int Negotiation { get; set; }
    public int Converted { get; set; }
    public int Lost { get; set; }
}

public class ConvertLeadDto
{
    public int? ClassId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
}

public class ConvertLeadResultDto
{
    public int LeadId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? AdmissionNumber { get; set; }
}

#endregion

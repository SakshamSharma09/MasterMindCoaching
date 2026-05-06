using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "Staff")]
public class AcademicPlannerController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<AcademicPlannerController> _logger;

    public AcademicPlannerController(MasterMindDbContext context, ILogger<AcademicPlannerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AcademicPlannerEntryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AcademicPlannerEntryDto>>>> GetEntries(
        [FromQuery] int? sessionId,
        [FromQuery] int? studentId,
        [FromQuery] string? schoolName,
        [FromQuery] string? examType)
    {
        try
        {
            await EnsureAcademicPlannerSchemaAsync();

            if (!sessionId.HasValue)
            {
                var activeSession = await _context.Sessions.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            var query = _context.AcademicPlannerEntries
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Class)
                .Where(e => !e.IsDeleted)
                .AsQueryable();

            if (sessionId.HasValue)
            {
                query = query.Where(e => e.SessionId == sessionId.Value);
            }

            if (studentId.HasValue)
            {
                query = query.Where(e => e.StudentId == studentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(schoolName))
            {
                var schoolLike = $"%{schoolName.Trim()}%";
                query = query.Where(e => EF.Functions.Like(e.SchoolName, schoolLike));
            }

            if (!string.IsNullOrWhiteSpace(examType) && Enum.TryParse<PlannerExamType>(examType, true, out var parsedType))
            {
                query = query.Where(e => e.ExamType == parsedType);
            }

            var entries = await query
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();

            var result = entries.Select(MapToDto).ToList();
            return Ok(new ApiResponse<IEnumerable<AcademicPlannerEntryDto>>
            {
                Success = true,
                Message = "Academic planner entries retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving academic planner entries");
            return StatusCode(500, new ApiResponse<IEnumerable<AcademicPlannerEntryDto>>
            {
                Success = false,
                Message = $"Error retrieving academic planner entries: {ex.Message}"
            });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AcademicPlannerEntryDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<AcademicPlannerEntryDto>>> Create([FromBody] CreateAcademicPlannerEntryRequest request)
    {
        try
        {
            await EnsureAcademicPlannerSchemaAsync();

            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Syllabus))
            {
                return BadRequest(new ApiResponse<AcademicPlannerEntryDto>
                {
                    Success = false,
                    Message = "Subject and syllabus are required"
                });
            }

            var targetSessionId = request.SessionId;
            if (!targetSessionId.HasValue)
            {
                var activeSession = await _context.Sessions.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                targetSessionId = activeSession?.Id;
            }

            var entry = new AcademicPlannerEntry
            {
                SessionId = targetSessionId,
                StudentId = request.StudentId,
                ClassId = request.ClassId,
                SchoolName = request.SchoolName?.Trim() ?? string.Empty,
                ExamType = ParseExamType(request.ExamType),
                Subject = request.Subject.Trim(),
                Syllabus = request.Syllabus.Trim(),
                ExamDate = request.ExamDate,
                StartTime = ParseTime(request.StartTime),
                EndTime = ParseTime(request.EndTime),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.AcademicPlannerEntries.Add(entry);
            await _context.SaveChangesAsync();

            entry = await _context.AcademicPlannerEntries
                .Include(e => e.Student)
                .Include(e => e.Class)
                .FirstAsync(e => e.Id == entry.Id);

            return CreatedAtAction(nameof(GetEntries), new { id = entry.Id }, new ApiResponse<AcademicPlannerEntryDto>
            {
                Success = true,
                Message = "Academic planner entry created successfully",
                Data = MapToDto(entry)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating academic planner entry");
            return StatusCode(500, new ApiResponse<AcademicPlannerEntryDto>
            {
                Success = false,
                Message = $"Error creating academic planner entry: {ex.Message}"
            });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AcademicPlannerEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AcademicPlannerEntryDto>>> Update(int id, [FromBody] UpdateAcademicPlannerEntryRequest request)
    {
        try
        {
            await EnsureAcademicPlannerSchemaAsync();

            var entry = await _context.AcademicPlannerEntries.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entry == null)
            {
                return NotFound(new ApiResponse<AcademicPlannerEntryDto>
                {
                    Success = false,
                    Message = "Entry not found"
                });
            }

            if (!string.IsNullOrWhiteSpace(request.SchoolName)) entry.SchoolName = request.SchoolName.Trim();
            if (!string.IsNullOrWhiteSpace(request.ExamType)) entry.ExamType = ParseExamType(request.ExamType);
            if (!string.IsNullOrWhiteSpace(request.Subject)) entry.Subject = request.Subject.Trim();
            if (!string.IsNullOrWhiteSpace(request.Syllabus)) entry.Syllabus = request.Syllabus.Trim();
            if (request.ExamDate.HasValue) entry.ExamDate = request.ExamDate.Value;
            if (request.StudentId.HasValue) entry.StudentId = request.StudentId;
            if (request.ClassId.HasValue) entry.ClassId = request.ClassId;
            if (request.StartTime != null) entry.StartTime = ParseTime(request.StartTime);
            if (request.EndTime != null) entry.EndTime = ParseTime(request.EndTime);
            if (request.Notes != null) entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            entry = await _context.AcademicPlannerEntries
                .Include(e => e.Student)
                .Include(e => e.Class)
                .FirstAsync(e => e.Id == id);

            return Ok(new ApiResponse<AcademicPlannerEntryDto>
            {
                Success = true,
                Message = "Academic planner entry updated successfully",
                Data = MapToDto(entry)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating academic planner entry {EntryId}", id);
            return StatusCode(500, new ApiResponse<AcademicPlannerEntryDto>
            {
                Success = false,
                Message = $"Error updating academic planner entry: {ex.Message}"
            });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            await EnsureAcademicPlannerSchemaAsync();

            var entry = await _context.AcademicPlannerEntries.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (entry == null)
            {
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Entry not found", Data = false });
            }

            entry.IsDeleted = true;
            entry.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool> { Success = true, Message = "Entry deleted successfully", Data = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting academic planner entry {EntryId}", id);
            return StatusCode(500, new ApiResponse<bool> { Success = false, Message = $"Error deleting entry: {ex.Message}", Data = false });
        }
    }

    private static PlannerExamType ParseExamType(string? examType)
    {
        if (string.IsNullOrWhiteSpace(examType)) return PlannerExamType.UnitTest;
        return Enum.TryParse<PlannerExamType>(examType, true, out var parsed) ? parsed : PlannerExamType.UnitTest;
    }

    private static TimeOnly? ParseTime(string? time)
    {
        if (string.IsNullOrWhiteSpace(time)) return null;
        return TimeOnly.TryParse(time, out var parsed) ? parsed : null;
    }

    private static AcademicPlannerEntryDto MapToDto(AcademicPlannerEntry e)
    {
        return new AcademicPlannerEntryDto
        {
            Id = e.Id,
            SessionId = e.SessionId,
            StudentId = e.StudentId,
            StudentName = e.Student != null ? $"{e.Student.FirstName} {e.Student.LastName}" : null,
            ClassId = e.ClassId,
            ClassName = e.Class?.Name,
            SchoolName = e.SchoolName,
            ExamType = e.ExamType.ToString(),
            Subject = e.Subject,
            Syllabus = e.Syllabus,
            ExamDate = e.ExamDate.ToString("yyyy-MM-dd"),
            StartTime = e.StartTime?.ToString("HH:mm"),
            EndTime = e.EndTime?.ToString("HH:mm"),
            Notes = e.Notes
        };
    }

    private async Task EnsureAcademicPlannerSchemaAsync()
    {
        if (!_context.Database.IsSqlServer())
        {
            return;
        }

        await _context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.AcademicPlannerEntries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcademicPlannerEntries
    (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SessionId int NULL,
        StudentId int NULL,
        ClassId int NULL,
        SchoolName nvarchar(200) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_SchoolName DEFAULT(''),
        ExamType int NOT NULL CONSTRAINT DF_AcademicPlannerEntries_ExamType DEFAULT(0),
        Subject nvarchar(200) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_Subject DEFAULT(''),
        Syllabus nvarchar(max) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_Syllabus DEFAULT(''),
        ExamDate date NOT NULL CONSTRAINT DF_AcademicPlannerEntries_ExamDate DEFAULT(CONVERT(date, SYSUTCDATETIME())),
        StartTime time NULL,
        EndTime time NULL,
        Notes nvarchar(2000) NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_AcademicPlannerEntries_CreatedAt DEFAULT(sysutcdatetime()),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_AcademicPlannerEntries_IsDeleted DEFAULT(0)
    );
END

IF COL_LENGTH('dbo.AcademicPlannerEntries', 'SessionId') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD SessionId int NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'StudentId') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD StudentId int NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'ClassId') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD ClassId int NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'SchoolName') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD SchoolName nvarchar(200) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_SchoolName_2 DEFAULT('');
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'ExamType') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD ExamType int NOT NULL CONSTRAINT DF_AcademicPlannerEntries_ExamType_2 DEFAULT(0);
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'Subject') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD Subject nvarchar(200) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_Subject_2 DEFAULT('');
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'Syllabus') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD Syllabus nvarchar(max) NOT NULL CONSTRAINT DF_AcademicPlannerEntries_Syllabus_2 DEFAULT('');
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'ExamDate') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD ExamDate date NOT NULL CONSTRAINT DF_AcademicPlannerEntries_ExamDate_2 DEFAULT(CONVERT(date, SYSUTCDATETIME()));
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'StartTime') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD StartTime time NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'EndTime') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD EndTime time NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'Notes') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD Notes nvarchar(2000) NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'CreatedAt') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_AcademicPlannerEntries_CreatedAt_2 DEFAULT(sysutcdatetime());
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'UpdatedAt') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD UpdatedAt datetime2 NULL;
IF COL_LENGTH('dbo.AcademicPlannerEntries', 'IsDeleted') IS NULL ALTER TABLE dbo.AcademicPlannerEntries ADD IsDeleted bit NOT NULL CONSTRAINT DF_AcademicPlannerEntries_IsDeleted_2 DEFAULT(0);
");
    }
}

public class AcademicPlannerEntryDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public string ExamDate { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
}

public class CreateAcademicPlannerEntryRequest
{
    public int? SessionId { get; set; }
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? SchoolName { get; set; }
    public string? ExamType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public DateOnly ExamDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAcademicPlannerEntryRequest
{
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? SchoolName { get; set; }
    public string? ExamType { get; set; }
    public string? Subject { get; set; }
    public string? Syllabus { get; set; }
    public DateOnly? ExamDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Notes { get; set; }
}

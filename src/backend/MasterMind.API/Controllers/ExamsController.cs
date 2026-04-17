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
public class ExamsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(MasterMindDbContext context, ILogger<ExamsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all exams with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExamDto>>>> GetExams(
        [FromQuery] int? classId,
        [FromQuery] int? subjectId,
        [FromQuery] string? status,
        [FromQuery] int? sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Exams
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.Session)
                .Where(e => !e.IsDeleted)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(e => e.ClassId == classId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(e => e.SubjectId == subjectId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ExamStatus>(status, true, out var examStatus))
            {
                query = query.Where(e => e.Status == examStatus);
            }

            if (sessionId.HasValue)
            {
                query = query.Where(e => e.SessionId == sessionId.Value);
            }

            var totalCount = await query.CountAsync();

            var exams = await query
                .OrderByDescending(e => e.ExamDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var examDtos = exams.Select(MapToDto).ToList();

            return Ok(new ApiResponse<IEnumerable<ExamDto>>
            {
                Success = true,
                Message = $"Retrieved {examDtos.Count} exams",
                Data = examDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exams");
            return StatusCode(500, new ApiResponse<IEnumerable<ExamDto>>
            {
                Success = false,
                Message = "Error retrieving exams"
            });
        }
    }

    /// <summary>
    /// Get a specific exam by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ExamDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExamDetailDto>>> GetExam(int id)
    {
        try
        {
            var exam = await _context.Exams
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .Include(e => e.Session)
                .Include(e => e.ExamResults)
                    .ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return NotFound(new ApiResponse<ExamDetailDto>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            return Ok(new ApiResponse<ExamDetailDto>
            {
                Success = true,
                Message = "Exam retrieved successfully",
                Data = MapToDetailDto(exam)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<ExamDetailDto>
            {
                Success = false,
                Message = "Error retrieving exam"
            });
        }
    }

    /// <summary>
    /// Create a new exam
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> CreateExam([FromBody] CreateExamDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new ApiResponse<ExamDto>
                {
                    Success = false,
                    Message = "Exam name is required"
                });
            }

            // Validate class exists
            var classEntity = await _context.Classes.FindAsync(dto.ClassId);
            if (classEntity == null)
            {
                return BadRequest(new ApiResponse<ExamDto>
                {
                    Success = false,
                    Message = "Class not found"
                });
            }

            // Get active session if not provided
            var sessionId = dto.SessionId;
            if (!sessionId.HasValue)
            {
                var activeSession = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            var exam = new Exam
            {
                Name = dto.Name,
                Description = dto.Description,
                ClassId = dto.ClassId,
                SubjectId = dto.SubjectId,
                Type = ParseExamType(dto.Type),
                ExamDate = dto.ExamDate,
                StartTime = dto.StartTime != null ? TimeOnly.Parse(dto.StartTime) : null,
                EndTime = dto.EndTime != null ? TimeOnly.Parse(dto.EndTime) : null,
                DurationMinutes = dto.DurationMinutes ?? 180,
                MaxMarks = dto.MaxMarks ?? 100,
                PassingMarks = dto.PassingMarks ?? 33,
                Syllabus = dto.Syllabus,
                Instructions = dto.Instructions,
                AcademicYear = dto.AcademicYear ?? GetCurrentAcademicYear(),
                SessionId = sessionId,
                Status = ExamStatus.Scheduled,
                CreatedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Reload with includes
            exam = await _context.Exams
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            return CreatedAtAction(nameof(GetExam), new { id = exam!.Id }, new ApiResponse<ExamDto>
            {
                Success = true,
                Message = "Exam created successfully",
                Data = MapToDto(exam)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam");
            return StatusCode(500, new ApiResponse<ExamDto>
            {
                Success = false,
                Message = "Error creating exam"
            });
        }
    }

    /// <summary>
    /// Update an existing exam
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> UpdateExam(int id, [FromBody] UpdateExamDto dto)
    {
        try
        {
            var exam = await _context.Exams
                .Include(e => e.Class)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return NotFound(new ApiResponse<ExamDto>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Name)) exam.Name = dto.Name;
            if (dto.Description != null) exam.Description = dto.Description;
            if (dto.ClassId.HasValue) exam.ClassId = dto.ClassId.Value;
            if (dto.SubjectId.HasValue) exam.SubjectId = dto.SubjectId.Value;
            if (!string.IsNullOrEmpty(dto.Type)) exam.Type = ParseExamType(dto.Type);
            if (dto.ExamDate.HasValue) exam.ExamDate = dto.ExamDate.Value;
            if (!string.IsNullOrEmpty(dto.StartTime)) exam.StartTime = TimeOnly.Parse(dto.StartTime);
            if (!string.IsNullOrEmpty(dto.EndTime)) exam.EndTime = TimeOnly.Parse(dto.EndTime);
            if (dto.DurationMinutes.HasValue) exam.DurationMinutes = dto.DurationMinutes.Value;
            if (dto.MaxMarks.HasValue) exam.MaxMarks = dto.MaxMarks.Value;
            if (dto.PassingMarks.HasValue) exam.PassingMarks = dto.PassingMarks.Value;
            if (dto.Syllabus != null) exam.Syllabus = dto.Syllabus;
            if (dto.Instructions != null) exam.Instructions = dto.Instructions;
            if (!string.IsNullOrEmpty(dto.Status)) exam.Status = ParseExamStatus(dto.Status);

            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<ExamDto>
            {
                Success = true,
                Message = "Exam updated successfully",
                Data = MapToDto(exam)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<ExamDto>
            {
                Success = false,
                Message = "Error updating exam"
            });
        }
    }

    /// <summary>
    /// Delete an exam (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteExam(int id)
    {
        try
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Exam not found",
                    Data = false
                });
            }

            exam.IsDeleted = true;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Exam deleted successfully",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error deleting exam"
            });
        }
    }

    /// <summary>
    /// Get exam results for a specific exam
    /// </summary>
    [HttpGet("{id}/results")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExamResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExamResultDto>>>> GetExamResults(int id)
    {
        try
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null || exam.IsDeleted)
            {
                return NotFound(new ApiResponse<IEnumerable<ExamResultDto>>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            var results = await _context.ExamResults
                .Include(r => r.Student)
                .Include(r => r.EvaluatedByUser)
                .Where(r => r.ExamId == id && !r.IsDeleted)
                .OrderByDescending(r => r.MarksObtained)
                .ToListAsync();

            var resultDtos = results.Select((r, index) => MapToResultDto(r, index + 1)).ToList();

            return Ok(new ApiResponse<IEnumerable<ExamResultDto>>
            {
                Success = true,
                Message = $"Retrieved {resultDtos.Count} results",
                Data = resultDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exam results for exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<IEnumerable<ExamResultDto>>
            {
                Success = false,
                Message = "Error retrieving exam results"
            });
        }
    }

    /// <summary>
    /// Add or update a result for a student
    /// </summary>
    [HttpPost("{id}/results")]
    [ProducesResponseType(typeof(ApiResponse<ExamResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExamResultDto>>> AddOrUpdateResult(int id, [FromBody] AddResultDto dto)
    {
        try
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null || exam.IsDeleted)
            {
                return NotFound(new ApiResponse<ExamResultDto>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null || student.IsDeleted)
            {
                return BadRequest(new ApiResponse<ExamResultDto>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            // Check if result already exists
            var existingResult = await _context.ExamResults
                .FirstOrDefaultAsync(r => r.ExamId == id && r.StudentId == dto.StudentId && !r.IsDeleted);

            if (existingResult != null)
            {
                // Update existing result
                existingResult.MarksObtained = dto.MarksObtained;
                existingResult.Grade = CalculateGrade(dto.MarksObtained, exam.MaxMarks);
                existingResult.Percentage = exam.MaxMarks > 0 ? (dto.MarksObtained / exam.MaxMarks) * 100 : 0;
                existingResult.IsPassed = dto.MarksObtained >= exam.PassingMarks;
                existingResult.Status = ResultStatus.Evaluated;
                existingResult.Remarks = dto.Remarks;
                existingResult.TeacherComments = dto.TeacherComments;
                existingResult.EvaluatedByUserId = GetCurrentUserId();
                existingResult.EvaluatedAt = DateTime.UtcNow;
                existingResult.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new result
                existingResult = new ExamResult
                {
                    ExamId = id,
                    StudentId = dto.StudentId,
                    MarksObtained = dto.MarksObtained,
                    Grade = CalculateGrade(dto.MarksObtained, exam.MaxMarks),
                    Percentage = exam.MaxMarks > 0 ? (dto.MarksObtained / exam.MaxMarks) * 100 : 0,
                    IsPassed = dto.MarksObtained >= exam.PassingMarks,
                    Status = ResultStatus.Evaluated,
                    Remarks = dto.Remarks,
                    TeacherComments = dto.TeacherComments,
                    EvaluatedByUserId = GetCurrentUserId(),
                    EvaluatedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ExamResults.Add(existingResult);
            }

            await _context.SaveChangesAsync();

            // Reload with includes
            existingResult = await _context.ExamResults
                .Include(r => r.Student)
                .Include(r => r.EvaluatedByUser)
                .FirstOrDefaultAsync(r => r.Id == existingResult.Id);

            return Ok(new ApiResponse<ExamResultDto>
            {
                Success = true,
                Message = "Result saved successfully",
                Data = MapToResultDto(existingResult!, null)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating result for exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<ExamResultDto>
            {
                Success = false,
                Message = "Error saving result"
            });
        }
    }

    /// <summary>
    /// Bulk add results for an exam
    /// </summary>
    [HttpPost("{id}/results/bulk")]
    [ProducesResponseType(typeof(ApiResponse<BulkResultResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BulkResultResponse>>> BulkAddResults(int id, [FromBody] BulkAddResultsDto dto)
    {
        try
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null || exam.IsDeleted)
            {
                return NotFound(new ApiResponse<BulkResultResponse>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            var successCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            foreach (var result in dto.Results)
            {
                try
                {
                    var student = await _context.Students.FindAsync(result.StudentId);
                    if (student == null || student.IsDeleted)
                    {
                        failedCount++;
                        errors.Add($"Student {result.StudentId} not found");
                        continue;
                    }

                    var existingResult = await _context.ExamResults
                        .FirstOrDefaultAsync(r => r.ExamId == id && r.StudentId == result.StudentId && !r.IsDeleted);

                    if (existingResult != null)
                    {
                        existingResult.MarksObtained = result.MarksObtained;
                        existingResult.Grade = CalculateGrade(result.MarksObtained, exam.MaxMarks);
                        existingResult.Percentage = exam.MaxMarks > 0 ? (result.MarksObtained / exam.MaxMarks) * 100 : 0;
                        existingResult.IsPassed = result.MarksObtained >= exam.PassingMarks;
                        existingResult.Status = ResultStatus.Evaluated;
                        existingResult.EvaluatedByUserId = GetCurrentUserId();
                        existingResult.EvaluatedAt = DateTime.UtcNow;
                        existingResult.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newResult = new ExamResult
                        {
                            ExamId = id,
                            StudentId = result.StudentId,
                            MarksObtained = result.MarksObtained,
                            Grade = CalculateGrade(result.MarksObtained, exam.MaxMarks),
                            Percentage = exam.MaxMarks > 0 ? (result.MarksObtained / exam.MaxMarks) * 100 : 0,
                            IsPassed = result.MarksObtained >= exam.PassingMarks,
                            Status = ResultStatus.Evaluated,
                            EvaluatedByUserId = GetCurrentUserId(),
                            EvaluatedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ExamResults.Add(newResult);
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    errors.Add($"Error for student {result.StudentId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<BulkResultResponse>
            {
                Success = true,
                Message = $"Processed {successCount + failedCount} results",
                Data = new BulkResultResponse
                {
                    SuccessCount = successCount,
                    FailedCount = failedCount,
                    Errors = errors
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk adding results for exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<BulkResultResponse>
            {
                Success = false,
                Message = "Error processing bulk results"
            });
        }
    }

    /// <summary>
    /// Publish exam results
    /// </summary>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> PublishResults(int id)
    {
        try
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null || exam.IsDeleted)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            // Update all evaluated results to published
            var results = await _context.ExamResults
                .Where(r => r.ExamId == id && r.Status == ResultStatus.Evaluated && !r.IsDeleted)
                .ToListAsync();

            foreach (var result in results)
            {
                result.Status = ResultStatus.Published;
                result.UpdatedAt = DateTime.UtcNow;
            }

            exam.Status = ExamStatus.Completed;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = $"Published {results.Count} results",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing results for exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error publishing results"
            });
        }
    }

    /// <summary>
    /// Get exam statistics
    /// </summary>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<ExamStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExamStatisticsDto>>> GetExamStatistics(int id)
    {
        try
        {
            var exam = await _context.Exams
                .Include(e => e.ExamResults)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (exam == null)
            {
                return NotFound(new ApiResponse<ExamStatisticsDto>
                {
                    Success = false,
                    Message = "Exam not found"
                });
            }

            var results = exam.ExamResults.Where(r => !r.IsDeleted && r.MarksObtained.HasValue).ToList();

            var stats = new ExamStatisticsDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                TotalStudents = results.Count,
                Passed = results.Count(r => r.IsPassed),
                Failed = results.Count(r => !r.IsPassed),
                Absent = exam.ExamResults.Count(r => r.Status == ResultStatus.Absent),
                HighestMarks = results.Any() ? results.Max(r => r.MarksObtained ?? 0) : 0,
                LowestMarks = results.Any() ? results.Min(r => r.MarksObtained ?? 0) : 0,
                AverageMarks = results.Any() ? results.Average(r => r.MarksObtained ?? 0) : 0,
                PassPercentage = results.Any() ? (decimal)results.Count(r => r.IsPassed) / results.Count * 100 : 0,
                MaxMarks = exam.MaxMarks,
                PassingMarks = exam.PassingMarks
            };

            return Ok(new ApiResponse<ExamStatisticsDto>
            {
                Success = true,
                Message = "Exam statistics retrieved successfully",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for exam {ExamId}", id);
            return StatusCode(500, new ApiResponse<ExamStatisticsDto>
            {
                Success = false,
                Message = "Error retrieving exam statistics"
            });
        }
    }

    #region Helper Methods

    private ExamDto MapToDto(Exam exam)
    {
        return new ExamDto
        {
            Id = exam.Id,
            Name = exam.Name,
            Description = exam.Description,
            ClassId = exam.ClassId,
            ClassName = exam.Class?.Name ?? "N/A",
            SubjectId = exam.SubjectId,
            SubjectName = exam.Subject?.Name,
            Type = exam.Type.ToString(),
            ExamDate = exam.ExamDate.ToString("yyyy-MM-dd"),
            StartTime = exam.StartTime?.ToString("HH:mm"),
            EndTime = exam.EndTime?.ToString("HH:mm"),
            DurationMinutes = exam.DurationMinutes,
            MaxMarks = exam.MaxMarks,
            PassingMarks = exam.PassingMarks,
            Status = exam.Status.ToString(),
            AcademicYear = exam.AcademicYear,
            ResultCount = exam.ExamResults?.Count ?? 0
        };
    }

    private ExamDetailDto MapToDetailDto(Exam exam)
    {
        return new ExamDetailDto
        {
            Id = exam.Id,
            Name = exam.Name,
            Description = exam.Description,
            ClassId = exam.ClassId,
            ClassName = exam.Class?.Name ?? "N/A",
            SubjectId = exam.SubjectId,
            SubjectName = exam.Subject?.Name,
            Type = exam.Type.ToString(),
            ExamDate = exam.ExamDate.ToString("yyyy-MM-dd"),
            StartTime = exam.StartTime?.ToString("HH:mm"),
            EndTime = exam.EndTime?.ToString("HH:mm"),
            DurationMinutes = exam.DurationMinutes,
            MaxMarks = exam.MaxMarks,
            PassingMarks = exam.PassingMarks,
            Syllabus = exam.Syllabus,
            Instructions = exam.Instructions,
            Status = exam.Status.ToString(),
            AcademicYear = exam.AcademicYear,
            Results = exam.ExamResults?
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.MarksObtained)
                .Select((r, i) => MapToResultDto(r, i + 1))
                .ToList() ?? new List<ExamResultDto>()
        };
    }

    private ExamResultDto MapToResultDto(ExamResult result, int? rank)
    {
        return new ExamResultDto
        {
            Id = result.Id,
            ExamId = result.ExamId,
            StudentId = result.StudentId,
            StudentName = result.Student != null ? $"{result.Student.FirstName} {result.Student.LastName}" : "Unknown",
            MarksObtained = result.MarksObtained,
            Grade = result.Grade,
            Percentage = result.Percentage,
            IsPassed = result.IsPassed,
            Rank = rank ?? result.Rank,
            Status = result.Status.ToString(),
            Remarks = result.Remarks,
            TeacherComments = result.TeacherComments,
            EvaluatedBy = result.EvaluatedByUser != null 
                ? $"{result.EvaluatedByUser.FirstName} {result.EvaluatedByUser.LastName}" 
                : null,
            EvaluatedAt = result.EvaluatedAt?.ToString("yyyy-MM-dd HH:mm")
        };
    }

    private static string CalculateGrade(decimal? marks, decimal maxMarks)
    {
        if (!marks.HasValue || maxMarks == 0) return "N/A";
        
        var percentage = (marks.Value / maxMarks) * 100;
        
        return percentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B+",
            >= 60 => "B",
            >= 50 => "C",
            >= 40 => "D",
            _ => "F"
        };
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string GetCurrentAcademicYear()
    {
        var now = DateTime.Now;
        var year = now.Month >= 4 ? now.Year : now.Year - 1;
        return $"{year}-{(year + 1) % 100:D2}";
    }

    private static ExamType ParseExamType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return ExamType.Written;
        
        return type.ToLower() switch
        {
            "written" => ExamType.Written,
            "oral" => ExamType.Oral,
            "practical" => ExamType.Practical,
            "online" => ExamType.Online,
            "assignment" => ExamType.Assignment,
            "project" => ExamType.Project,
            "quiz" => ExamType.Quiz,
            "midterm" or "mid-term" => ExamType.MidTerm,
            "final" => ExamType.Final,
            _ => ExamType.Written
        };
    }

    private static ExamStatus ParseExamStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return ExamStatus.Scheduled;
        
        return status.ToLower() switch
        {
            "scheduled" => ExamStatus.Scheduled,
            "inprogress" or "in-progress" => ExamStatus.InProgress,
            "completed" => ExamStatus.Completed,
            "cancelled" => ExamStatus.Cancelled,
            "postponed" => ExamStatus.Postponed,
            _ => ExamStatus.Scheduled
        };
    }

    #endregion
}

#region DTOs

public class ExamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ExamDate { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int ResultCount { get; set; }
}

public class ExamDetailDto : ExamDto
{
    public string? Syllabus { get; set; }
    public string? Instructions { get; set; }
    public List<ExamResultDto> Results { get; set; } = new();
}

public class CreateExamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ClassId { get; set; }
    public int? SubjectId { get; set; }
    public string? Type { get; set; }
    public DateTime ExamDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? PassingMarks { get; set; }
    public string? Syllabus { get; set; }
    public string? Instructions { get; set; }
    public string? AcademicYear { get; set; }
    public int? SessionId { get; set; }
}

public class UpdateExamDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ClassId { get; set; }
    public int? SubjectId { get; set; }
    public string? Type { get; set; }
    public DateTime? ExamDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? PassingMarks { get; set; }
    public string? Syllabus { get; set; }
    public string? Instructions { get; set; }
    public string? Status { get; set; }
}

public class ExamResultDto
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal? MarksObtained { get; set; }
    public string? Grade { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsPassed { get; set; }
    public int? Rank { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? TeacherComments { get; set; }
    public string? EvaluatedBy { get; set; }
    public string? EvaluatedAt { get; set; }
}

public class AddResultDto
{
    public int StudentId { get; set; }
    public decimal MarksObtained { get; set; }
    public string? Remarks { get; set; }
    public string? TeacherComments { get; set; }
}

public class BulkAddResultsDto
{
    public List<BulkResultItem> Results { get; set; } = new();
}

public class BulkResultItem
{
    public int StudentId { get; set; }
    public decimal MarksObtained { get; set; }
}

public class BulkResultResponse
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ExamStatisticsDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Absent { get; set; }
    public decimal HighestMarks { get; set; }
    public decimal LowestMarks { get; set; }
    public decimal AverageMarks { get; set; }
    public decimal PassPercentage { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassingMarks { get; set; }
}

#endregion

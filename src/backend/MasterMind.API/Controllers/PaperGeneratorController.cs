using System.Security.Claims;
using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.PaperGenerator;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Implementations;
using MasterMind.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/paper-generator")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class PaperGeneratorController : ControllerBase
{
    private const string SourceContainer = "paper-source-documents";

    private readonly MasterMindDbContext _context;
    private readonly IPaperDocumentStorageService _storage;
    private readonly IPaperGenerationService _paperGenerationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaperGeneratorController> _logger;

    public PaperGeneratorController(
        MasterMindDbContext context,
        IPaperDocumentStorageService storage,
        IPaperGenerationService paperGenerationService,
        IConfiguration configuration,
        ILogger<PaperGeneratorController> logger)
    {
        _context = context;
        _storage = storage;
        _paperGenerationService = paperGenerationService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("documents")]
    [RequestSizeLimit(150_000_000)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaperDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaperDocumentDto>>>> UploadDocuments(
        [FromForm] List<IFormFile> files,
        [FromForm] int? sessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!_storage.IsConfigured)
            {
                return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>>
                {
                    Success = false,
                    Message = "Azure Blob Storage is not configured for Paper Generator."
                });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = "Upload at least one PDF." });
            }

            var maxFiles = _configuration.GetValue("PaperGenerator:MaxFiles", 5);
            var maxFileSizeBytes = _configuration.GetValue("PaperGenerator:MaxFileSizeMb", 25) * 1024L * 1024L;
            if (files.Count > maxFiles)
            {
                return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"Upload at most {maxFiles} PDFs at a time." });
            }

            var adminUserId = GetCurrentUserId();
            var targetSessionId = sessionId ?? await GetActiveSessionIdAsync(cancellationToken);
            var sourceRetentionDays = _configuration.GetValue("PaperGenerator:SourceRetentionDays", 7);
            var uploaded = new List<PaperDocument>();
            var uploadBatchId = Guid.NewGuid().ToString("N");

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"{file.FileName} is empty." });
                }

                if (file.Length > maxFileSizeBytes)
                {
                    return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"{file.FileName} exceeds the {maxFileSizeBytes / 1024 / 1024} MB limit." });
                }

                var extension = Path.GetExtension(file.FileName);
                if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"{file.FileName} must be a PDF file." });
                }

                var safeFileName = Path.GetFileName(file.FileName);
                var blobName = $"paper-generator/{targetSessionId?.ToString() ?? "global"}/{adminUserId}/uploads/{uploadBatchId}/{Guid.NewGuid():N}.pdf";
                await using var stream = file.OpenReadStream();
                if (!await HasPdfHeaderAsync(stream, cancellationToken))
                {
                    return BadRequest(new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"{file.FileName} is not a valid PDF file." });
                }

                stream.Position = 0;
                await _storage.UploadAsync(SourceContainer, blobName, stream, "application/pdf", cancellationToken);

                uploaded.Add(new PaperDocument
                {
                    SessionId = targetSessionId,
                    UploadedByUserId = adminUserId,
                    FileName = safeFileName,
                    ContentType = "application/pdf",
                    SizeBytes = file.Length,
                    BlobContainer = SourceContainer,
                    BlobName = blobName,
                    Status = PaperDocumentStatus.Uploaded,
                    UploadedAt = DateTime.UtcNow,
                    RetainUntil = DateTime.UtcNow.AddDays(sourceRetentionDays),
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.PaperDocuments.AddRange(uploaded);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ApiResponse<IEnumerable<PaperDocumentDto>>
            {
                Success = true,
                Message = "PDFs uploaded successfully.",
                Data = uploaded.Select(PaperGenerationService.MapDocument).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paper document upload failed");
            return StatusCode(500, new ApiResponse<IEnumerable<PaperDocumentDto>> { Success = false, Message = $"Upload failed: {ex.Message}" });
        }
    }

    [HttpGet("documents")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaperDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaperDocumentDto>>>> GetDocuments([FromQuery] int? sessionId, CancellationToken cancellationToken)
    {
        var targetSessionId = sessionId ?? await GetActiveSessionIdAsync(cancellationToken);
        var query = _context.PaperDocuments.AsNoTracking().Where(d => !d.IsDeleted);
        if (targetSessionId.HasValue)
        {
            query = query.Where(d => d.SessionId == targetSessionId.Value);
        }

        var documents = await query
            .OrderByDescending(d => d.UploadedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return Ok(new ApiResponse<IEnumerable<PaperDocumentDto>>
        {
            Success = true,
            Message = "Paper documents retrieved successfully.",
            Data = documents.Select(PaperGenerationService.MapDocument).ToList()
        });
    }

    [HttpPost("jobs")]
    [ProducesResponseType(typeof(ApiResponse<PaperGenerationJobSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaperGenerationJobSummary>>> CreateJob([FromBody] CreatePaperGenerationJobRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _paperGenerationService.CreateAndRunJobAsync(request, GetCurrentUserId(), cancellationToken);
            return Ok(new ApiResponse<PaperGenerationJobSummary> { Success = true, Message = "Paper generated successfully.", Data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paper generation failed");
            return BadRequest(new ApiResponse<PaperGenerationJobSummary> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaperGenerationJobDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaperGenerationJobDto>>>> GetJobs([FromQuery] int? sessionId, CancellationToken cancellationToken)
    {
        var jobs = await _paperGenerationService.GetRecentJobsAsync(sessionId, GetCurrentUserId(), cancellationToken);
        return Ok(new ApiResponse<IEnumerable<PaperGenerationJobDto>> { Success = true, Message = "Paper jobs retrieved successfully.", Data = jobs });
    }

    [HttpGet("jobs/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PaperGenerationJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaperGenerationJobDto>>> GetJob(int id, CancellationToken cancellationToken)
    {
        var job = await _paperGenerationService.GetJobAsync(id, GetCurrentUserId(), cancellationToken);
        if (job == null)
        {
            return NotFound(new ApiResponse<PaperGenerationJobDto> { Success = false, Message = "Paper generation job not found." });
        }

        return Ok(new ApiResponse<PaperGenerationJobDto> { Success = true, Message = "Paper job retrieved successfully.", Data = job });
    }

    [HttpGet("jobs/{id:int}/paper")]
    public async Task<IActionResult> DownloadPaper(int id, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _paperGenerationService.DownloadGeneratedFileAsync(id, PaperGeneratedFileKind.Paper, GetCurrentUserId(), cancellationToken);
        return File(content, "application/pdf", fileName);
    }

    [HttpGet("jobs/{id:int}/answer-key")]
    public async Task<IActionResult> DownloadAnswerKey(int id, CancellationToken cancellationToken)
    {
        var (content, fileName) = await _paperGenerationService.DownloadGeneratedFileAsync(id, PaperGeneratedFileKind.AnswerKey, GetCurrentUserId(), cancellationToken);
        return File(content, "application/pdf", fileName);
    }

    [HttpGet("questions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaperQuestionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaperQuestionDto>>>> GetQuestions(
        [FromQuery] int? sessionId,
        [FromQuery] string? subject,
        [FromQuery] string? className,
        [FromQuery] string? chapter,
        CancellationToken cancellationToken)
    {
        var targetSessionId = sessionId ?? await GetActiveSessionIdAsync(cancellationToken);
        var questions = await _paperGenerationService.SearchQuestionsAsync(targetSessionId, subject, className, chapter, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<PaperQuestionDto>> { Success = true, Message = "Questions retrieved successfully.", Data = questions });
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue("uid")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(id, out var userId))
        {
            throw new UnauthorizedAccessException("Unable to identify current user.");
        }

        return userId;
    }

    private async Task<int?> GetActiveSessionIdAsync(CancellationToken cancellationToken)
    {
        return await _context.Sessions
            .Where(s => s.IsActive && !s.IsDeleted)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<bool> HasPdfHeaderAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[5];
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        return bytesRead == 5 && header[0] == '%' && header[1] == 'P' && header[2] == 'D' && header[3] == 'F' && header[4] == '-';
    }
}

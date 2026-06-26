using MasterMind.API.Models.DTOs.PaperGenerator;
using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

public interface IPaperGenerationService
{
    Task<PaperGenerationJobSummary> CreateAndRunJobAsync(CreatePaperGenerationJobRequest request, int adminUserId, CancellationToken cancellationToken = default);
    Task<PaperGenerationJobDto?> GetJobAsync(int jobId, int adminUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaperGenerationJobDto>> GetRecentJobsAsync(int? sessionId, int adminUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaperQuestionDto>> SearchQuestionsAsync(int? sessionId, string? subject, string? className, string? chapter, CancellationToken cancellationToken = default);
    Task<(Stream Content, string FileName)> DownloadGeneratedFileAsync(int jobId, PaperGeneratedFileKind kind, int adminUserId, CancellationToken cancellationToken = default);
}

public enum PaperGeneratedFileKind
{
    Paper,
    AnswerKey
}

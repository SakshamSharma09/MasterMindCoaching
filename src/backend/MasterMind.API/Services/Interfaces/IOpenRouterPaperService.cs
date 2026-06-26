using MasterMind.API.Models.DTOs.PaperGenerator;
using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

public interface IOpenRouterPaperService
{
    Task<IReadOnlyList<PaperExtractedQuestion>> GenerateQuestionsAsync(
        CreatePaperGenerationJobRequest request,
        int questionCount,
        int? sessionId,
        CancellationToken cancellationToken = default);
}

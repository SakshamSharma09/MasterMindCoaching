using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MasterMind.API.Models.DTOs.PaperGenerator;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

public class OpenRouterPaperService : IOpenRouterPaperService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenRouterPaperService> _logger;

    public OpenRouterPaperService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenRouterPaperService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PaperExtractedQuestion>> GenerateQuestionsAsync(
        CreatePaperGenerationJobRequest request,
        int questionCount,
        int? sessionId,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || questionCount <= 0)
        {
            return Array.Empty<PaperExtractedQuestion>();
        }

        var model = _configuration["OpenRouter:DefaultModel"] ?? "deepseek/deepseek-chat";
        var fallbackModel = _configuration["OpenRouter:FallbackModel"] ?? "z-ai/glm-4.5";

        try
        {
            return await GenerateWithModelAsync(model, apiKey, request, questionCount, sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenRouter paper generation failed with {Model}; retrying fallback {FallbackModel}", model, fallbackModel);
            try
            {
                return await GenerateWithModelAsync(fallbackModel, apiKey, request, questionCount, sessionId, cancellationToken);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogWarning(fallbackEx, "OpenRouter fallback failed; deterministic generator will be used");
                return Array.Empty<PaperExtractedQuestion>();
            }
        }
    }

    private async Task<IReadOnlyList<PaperExtractedQuestion>> GenerateWithModelAsync(
        string model,
        string apiKey,
        CreatePaperGenerationJobRequest request,
        int questionCount,
        int? sessionId,
        CancellationToken cancellationToken)
    {
        var timeoutSeconds = _configuration.GetValue("OpenRouter:TimeoutSeconds", 90);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Add("HTTP-Referer", "https://victorious-glacier-0e6507000.6.azurestaticapps.net");
        httpRequest.Headers.Add("X-Title", "MasterMind Coaching Paper Generator");

        var prompt = $"""
            Generate {questionCount} exam questions strictly as JSON for MasterMind Coaching Classes.
            Class: {request.ClassName}
            Subject: {request.Subject}
            Chapter: {request.Chapter ?? "Full syllabus"}
            Difficulty split: Easy {request.EasyPercentage}%, Medium {request.MediumPercentage}%, Hard {request.HardPercentage}%.
            Return only a JSON array. Each item must have: questionText, answerText, marks, questionType, difficulty.
            Allowed questionType values: Mcq, OneMark, TwoMark, FiveMark, CaseStudy.
            Allowed difficulty values: Easy, Medium, Hard.
            """;

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = "You create exam papers from provided academic constraints. Return valid JSON only." },
                new { role = "user", content = prompt }
            },
            temperature = 0.35
        };

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        using var root = JsonDocument.Parse(json);
        var content = root.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<PaperExtractedQuestion>();
        }

        var parsed = JsonSerializer.Deserialize<List<OpenRouterQuestion>>(StripCodeFence(content), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<OpenRouterQuestion>();

        return parsed
            .Where(q => !string.IsNullOrWhiteSpace(q.QuestionText))
            .Take(questionCount)
            .Select(q => new PaperExtractedQuestion
            {
                SessionId = sessionId,
                Subject = request.Subject.Trim(),
                ClassName = request.ClassName.Trim(),
                Chapter = string.IsNullOrWhiteSpace(request.Chapter) ? "General" : request.Chapter.Trim(),
                Marks = Math.Clamp(q.Marks, 1, 10),
                QuestionType = Enum.TryParse<PaperQuestionType>(q.QuestionType, true, out var type) ? type : PaperQuestionType.ShortAnswer,
                Difficulty = Enum.TryParse<PaperQuestionDifficulty>(q.Difficulty, true, out var difficulty) ? difficulty : PaperQuestionDifficulty.Medium,
                QuestionText = q.QuestionText.Trim(),
                AnswerText = string.IsNullOrWhiteSpace(q.AnswerText) ? "Teacher evaluation required." : q.AnswerText.Trim(),
                SourceMode = $"OpenRouter:{model}",
                CreatedAt = DateTime.UtcNow
            })
            .ToList();
    }

    private static string StripCodeFence(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;

        var firstLineEnd = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstLineEnd < 0 || lastFence <= firstLineEnd) return trimmed;

        return trimmed[(firstLineEnd + 1)..lastFence].Trim();
    }

    private sealed class OpenRouterQuestion
    {
        public string QuestionText { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public int Marks { get; set; } = 1;
        public string QuestionType { get; set; } = "ShortAnswer";
        public string Difficulty { get; set; } = "Medium";
    }
}

using sloppr.Models;
using sloppr.Utilities;

namespace sloppr.DTOs;

public class IngredientExtractionResponse
{

    public IngredientExtractionResponse(AiModel model, string systemPrompt)
    {
        if (model != null)
        {
            this.AiModelId = model.Id;
            this.AiModel = model;
        }
        this.SystemPrompt = systemPrompt;
        this.AiModel?.AiProvider = null;
    }

    public string SystemPrompt { get; set; }

    public int? AiModelId { get; set; }
    public AiModel? AiModel { get; set; }

    public List<IngredientChallenge> Challenges { get; set; } = new();
}

public class IngredientChallenge
{

    public IngredientChallenge(string prompt, List<string> expectedResponse, string actualResponse, long? inputTokenCount, long? outputTokenCount)
    {
        this.ActualResponse = LlmArrayResponse.Read(actualResponse);
        this.ExpectedResponse = expectedResponse;
        this.Prompt = prompt;
        this.InputTokenCount = inputTokenCount;
        this.OutputTokenCount = outputTokenCount;
    }

    public string Prompt { get; set; } = string.Empty;
    public List<string> ExpectedResponse { get; set; } = new();
    public List<string> ActualResponse { get; init; }

    public long? InputTokenCount { get; set; }
    public long? OutputTokenCount { get; set; }

    public bool IsExactMatch => ExpectedResponse?.SequenceEqual(ActualResponse, StringComparer.OrdinalIgnoreCase) ?? false;

    public int? Score { get; set; }
}
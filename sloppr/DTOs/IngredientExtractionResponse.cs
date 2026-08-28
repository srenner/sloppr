using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using sloppr.Models;
using sloppr.Settings;

namespace sloppr.DTOs;

public class IngredientExtractionResponse(IOptions<AISettings> aiSettings)
{

    private readonly string systemPrompt = aiSettings.Value.DefaultIngredientExtractionPrompt;
    private readonly List<ExtractionChallenge> extractionChallenges = aiSettings.Value.ExtractionChallenges;

    public string SystemPrompt { get; set; }

    public int? AiModelId { get; init; }
    public AiModel? AiModel { get; init; }

    public List<string> ExpectedOutput { get; init; }
    public List<string> ActualOutput { get; init; }

    public int? InputTokenCount { get; init; }
    public int? OutputTokenCount { get; init; }

    public double Score { get; init; }
}

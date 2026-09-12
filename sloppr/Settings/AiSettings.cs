namespace sloppr.Settings;

public class AISettings
{
    public string DefaultCuisineExtractionPrompt { get; set; } = string.Empty;
    public string DefaultIngredientExtractionPrompt { get; set; } = string.Empty;
    public List<ExtractionChallenge> ExtractionChallenges { get; set; } = new();
    public string IdeaGenerationPrompt { get; set; } = string.Empty;
}

public class ExtractionChallenge
{
    public string Prompt { get; set; } = string.Empty;
    public List<string> ExpectedResponse { get; set; } = new();
}
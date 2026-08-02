namespace sloppr.Settings;

public class AISettings
{
    public string DefaultIngredientExtractionPrompt { get; set; } = string.Empty;
    public List<ExtractionChallenge> ExtractionChallenges { get; set; } = new();
}

public class ExtractionChallenge
{
    public string Prompt { get; set; } = string.Empty;
    public List<string> ExpectedResponse { get; set; } = new();
}
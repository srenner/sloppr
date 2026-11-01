namespace sloppr.DTOs;

public class RecipeGenerationResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Difficulty { get; set; }
    public string? Notes { get; set; }
    public ICollection<RecipeIngredientRequest> Ingredients { get; set; } = [];
    public ICollection<RecipeStepRequest> Steps { get; set; } = [];
}

namespace sloppr.DTOs;

public class RecipeRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Difficulty { get; set; }
    public string? Notes { get; set; }
    public int? MealIdeaId { get; set; }
    public ICollection<RecipeIngredientRequest> Ingredients { get; set; } = [];
    public ICollection<RecipeStepRequest> Steps { get; set; } = [];
}

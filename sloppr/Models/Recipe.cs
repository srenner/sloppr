namespace sloppr.Models;

/// <summary>
/// Full meal recipe
/// </summary>
public class Recipe : BaseModel
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int? Difficulty { get; set; }
    public string? Notes { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    public ICollection<RecipeStep> Steps { get; set; } = [];

    public int? MealIdeaId { get; set; }
    public MealIdea? MealIdea { get; set; }

    /// <summary>
    /// Optional per-recipe template override. Falls back to the global default.
    /// </summary>
    public string? TemplateName { get; set; }
}

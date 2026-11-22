using sloppr.Enums;

namespace sloppr.Models;

/// <summary>
/// Full meal recipe. Content is stored as JSON in <see cref="ContentJson"/>.
/// </summary>
public class Recipe : BaseModel
{
    public string ContentJson { get; set; } = string.Empty;
    public RecipeSourceType SourceType { get; set; } = RecipeSourceType.Manual;

    public int? MealIdeaId { get; set; }
    public MealIdea? MealIdea { get; set; }

    /// <summary>
    /// Optional per-recipe template override. Falls back to the global default.
    /// </summary>
    public string? TemplateName { get; set; }
}

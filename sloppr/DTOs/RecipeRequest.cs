using sloppr.Content;
using sloppr.Enums;

namespace sloppr.DTOs;

public class RecipeRequest
{
    public RecipeContent Content { get; set; } = new();
    public int? MealIdeaId { get; set; }
    public string? TemplateName { get; set; }
    public RecipeSourceType? SourceType { get; set; }
}

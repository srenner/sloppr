using sloppr.Enums;

namespace sloppr.DTOs;

public class RecipeSummaryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecipeSourceType SourceType { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int? Difficulty { get; set; }
    public DateTime DateUpdated { get; set; }
}

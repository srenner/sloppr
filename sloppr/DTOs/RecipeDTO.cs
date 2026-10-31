using sloppr.Recipes;

namespace sloppr.DTOs;

public class RecipeDTO
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public string CreatedBy { get; set; } = "system";
    public string UpdatedBy { get; set; } = "system";

    public required string Name { get; set; }
    public required string Description { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Difficulty { get; set; }
    public string? Notes { get; set; }

    public string FormatKey { get; set; } = RecipeFormat.Key;
    public int FormatVersion { get; set; } = RecipeFormat.Version;

    public int? MealIdeaId { get; set; }
    public ICollection<RecipeIngredientDTO> Ingredients { get; set; } = [];
    public ICollection<RecipeStepDTO> Steps { get; set; } = [];
}

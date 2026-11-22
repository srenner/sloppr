using sloppr.Content;
using sloppr.Enums;

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

    public int? MealIdeaId { get; set; }
    public string? TemplateName { get; set; }
    public RecipeSourceType SourceType { get; set; }
    public required RecipeContent Content { get; set; } = new();
}

namespace sloppr.Models;

/// <summary>
/// User-owned markdown template used to render a recipe.
/// </summary>
public class RecipeTemplate : BaseModel
{
    public required string Name { get; set; }
    public required string Content { get; set; }
}

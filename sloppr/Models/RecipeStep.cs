namespace sloppr.Models;

public class RecipeStep
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int Order { get; set; }
    public required string Instruction { get; set; }
    public string? Group { get; set; }
}

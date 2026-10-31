namespace sloppr.DTOs;

public class RecipeStepDTO
{
    public int Id { get; set; }
    public int Order { get; set; }
    public required string Instruction { get; set; }
    public string? Group { get; set; }
}

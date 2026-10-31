namespace sloppr.DTOs;

public class RecipeStepRequest
{
    public int Order { get; set; }
    public required string Instruction { get; set; }
    public string? Group { get; set; }
}

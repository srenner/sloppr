namespace sloppr.DTOs;

public class RecipeIngredientRequest
{
    public int Order { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public required string Name { get; set; }
    public string? Group { get; set; }
    public string? Note { get; set; }
}

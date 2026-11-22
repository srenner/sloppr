namespace sloppr.Content;

public class RecipeContent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ServingCount { get; set; } = 4;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int? Difficulty { get; set; }
    public string? Notes { get; set; }

    public List<RecipeIngredientContent> Ingredients { get; set; } = [];
    public List<RecipeStepContent> Steps { get; set; } = [];
}

public class RecipeIngredientContent
{
    public string Name { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Group { get; set; }
    public string? Note { get; set; }
}

public class RecipeStepContent
{
    public string Instruction { get; set; } = string.Empty;
    public string? Group { get; set; }
}

namespace sloppr.Models;

/// <summary>
/// Full meal recipe
/// </summary>
public class Recipe : BaseModel
{

    public string Name { get; set; }
    public string Description { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Difficulty { get; set; }

    //public List<RecipeIngredient> Ingredients { get; set; }
    //public List<RecipeStep> Steps { get; set; }
}

public class RecipeIngredient
{
    public double Quantity { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Name { get; set; }
}

public class RecipeStep
{

}


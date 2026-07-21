namespace sloppr.Models;

/// <summary>
/// Full meal recipe
/// </summary>
public class Recipe : BaseModel
{
    public required string FullText { get; set; }
    public string TextFormat { get; set; } = "md";
    public int PrepTime { get; set; } = 0;
    public int CookTime { get; set; } = 0;

}

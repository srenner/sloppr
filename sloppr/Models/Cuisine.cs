namespace sloppr.Models;

/// <summary>
/// A cuisine type extracted from a user prompt
/// </summary>
public class Cuisine : BaseModel
{
    public required string Name { get; set; }
    public int NumQueried { get; set; }
    public int NumUsed { get; set; }
    public DateTime? LastUsed { get; set; }
}

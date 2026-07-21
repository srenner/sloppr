using System;

namespace sloppr.Models;

/// <summary>
/// An ingredient the user specifically requested for use
/// </summary>
public class KeyIngredient : BaseModel
{
    public required string Name { get; set; }
    public int NumQueried { get; set; }
    public int NumUsed { get; set; }
    public DateTime? LastUsed { get; set; }
}


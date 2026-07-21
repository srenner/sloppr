using System;

namespace sloppr.DTOs;

public class KeyIngredientDTO
{
    public required string Name { get; set; }
    public int NumQueried { get; set; }
    public int NumUsed { get; set; }
    public DateTime? LastUsed { get; set; }
}
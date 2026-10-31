namespace sloppr.Models;

/// <summary>
/// A published version of a recipe format and its rules. Rows are append-only: a change
/// creates a new version instead of editing an existing one.
/// </summary>
public class RecipeFormatVersion
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public int Version { get; set; }

    /// <summary>Format rules serialized as JSON.</summary>
    public required string Rules { get; set; }

    public bool IsCurrent { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
}

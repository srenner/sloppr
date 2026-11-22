namespace sloppr.Models;

/// <summary>Immutable recipe snapshot. Append-only.</summary>
public class CookbookEntryVersion : BaseModel
{
    public int CookbookEntryId { get; set; }
    public CookbookEntry? CookbookEntry { get; set; }

    public int VersionNumber { get; set; }
    public string ContentJson { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int? Difficulty { get; set; }
    public string? ChangeNote { get; set; }
}

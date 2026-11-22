namespace sloppr.Models;

public class CookbookEntry : BaseModel
{
    public int CookbookId { get; set; }
    public Cookbook? Cookbook { get; set; }

    /// <summary>Provenance only; the entry keeps its own snapshots.</summary>
    public int? SourceRecipeId { get; set; }
    public Recipe? SourceRecipe { get; set; }

    public int CurrentVersionNumber { get; set; } = 1;

    public ICollection<CookbookEntryVersion> Versions { get; set; } = [];
}

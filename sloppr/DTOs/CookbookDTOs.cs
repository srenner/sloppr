using sloppr.Content;

namespace sloppr.DTOs;

public class CookbookDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class CookbookEntryDTO
{
    public int Id { get; set; }
    public int CookbookId { get; set; }
    public int? SourceRecipeId { get; set; }
    public int CurrentVersionNumber { get; set; }
    public required RecipeContent Content { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class CookbookEntrySummaryDTO
{
    public int Id { get; set; }
    public int CookbookId { get; set; }
    public int? SourceRecipeId { get; set; }
    public int CurrentVersionNumber { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int ServingCount { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int? Difficulty { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class CookbookEntryRequest
{
    public RecipeContent Content { get; set; } = new();
    public string? ChangeNote { get; set; }
}

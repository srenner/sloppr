using sloppr.DTOs;

namespace sloppr.Services;

public interface ICookbookService
{
    Task EnsureSeededAsync();
    Task<IReadOnlyList<CookbookDTO>> GetCookbooksAsync();
    Task<IReadOnlyList<CookbookEntrySummaryDTO>?> GetEntriesAsync(int cookbookId);
    Task<CookbookEntryDTO?> GetEntryAsync(int cookbookId, int entryId, int? version);
    Task<CookbookEntryDTO?> CreateEntryAsync(int cookbookId, CookbookEntryRequest request);
    Task<(CookbookEntryDTO Entry, bool Created)?> CopyFromRecipeAsync(int cookbookId, int recipeId);
    Task<CookbookEntryDTO?> UpdateEntryAsync(int cookbookId, int entryId, CookbookEntryRequest request);
    Task<bool> DeleteEntryAsync(int cookbookId, int entryId);
}

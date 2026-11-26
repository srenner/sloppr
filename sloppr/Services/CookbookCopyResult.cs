using sloppr.DTOs;

namespace sloppr.Services;

public record CookbookCopyResult(CookbookEntryDTO Entry, bool Created);

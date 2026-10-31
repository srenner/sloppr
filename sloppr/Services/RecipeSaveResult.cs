using sloppr.Models;

namespace sloppr.Services;

public record RecipeSaveResult(Recipe? Recipe, IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

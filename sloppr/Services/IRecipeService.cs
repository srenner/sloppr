using sloppr.Models;

namespace sloppr.Services;

public interface IRecipeService
{
    public Task<IReadOnlyList<Recipe>> GetAllAsync();
    public Task<Recipe?> GetByIdAsync(int id);
    public Task<RecipeFormatVersion?> GetCurrentFormatAsync();
    public Task<RecipeSaveResult> AddAsync(Recipe recipe);
    public Task<RecipeSaveResult> UpdateAsync(Recipe recipe);
    public Task DeleteAsync(Recipe recipe);
}

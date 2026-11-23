using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public interface IRecipeService
{
    public Task<IReadOnlyList<Recipe>> GetAllAsync();
    public Task<RecipePageDTO> GetPagedAsync(int page, int pageSize);
    public Task<Recipe?> GetByIdAsync(int id);
    public Task AddAsync(Recipe recipe);
    public Task UpdateAsync(Recipe recipe);
    public Task DeleteAsync(Recipe recipe);
}

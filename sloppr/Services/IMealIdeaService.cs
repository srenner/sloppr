using sloppr.Models;

namespace sloppr.Services;

public interface IMealIdeaService
{
    public Task<MealIdea?> GetByIdAsync(int id);
    public Task<MealIdea?> GetByIdWithIngredientsAsync(int id);
    public Task<IEnumerable<MealIdea>> GetFilteredAsync();
    public Task<IEnumerable<MealIdea>> GetAllAsync();
    public Task<IEnumerable<MealIdea>> CreateRangeAsync(IEnumerable<string> names, IEnumerable<int> keyIngredientIds);
    public Task<MealIdea> UpdateAsync(MealIdea idea);
}

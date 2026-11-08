using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public interface IRecipeTemplateService
{
    Task<string> RenderAsync(RecipeDTO recipe, string? templateName = null);

    Task<IReadOnlyList<RecipeTemplate>> GetAllAsync();

    Task<RecipeTemplate?> GetByNameAsync(string name);

    Task<RecipeTemplate> CreateAsync(RecipeTemplateRequest request);

    Task<RecipeTemplate?> UpdateAsync(string name, string content);

    Task<bool> DeleteAsync(string name);

    Task SeedDefaultAsync();

    string GetStarterContent();
}

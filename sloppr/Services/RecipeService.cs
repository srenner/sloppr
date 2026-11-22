using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _db;

    public RecipeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        return await _db.Recipes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Recipe recipe)
    {
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        _db.Recipes.Update(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Recipe recipe)
    {
        recipe.IsDeleted = true;
        _db.Recipes.Update(recipe);
        await _db.SaveChangesAsync();
    }
}

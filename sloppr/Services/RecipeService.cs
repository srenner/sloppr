using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _db;

    public RecipeService(IUnitOfWork uow, ApplicationDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        return await _db.Recipes
            .Include(r => r.Ingredients.OrderBy(i => i.Order))
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _db.Recipes
            .Include(r => r.Ingredients.OrderBy(i => i.Order))
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Recipe recipe)
    {
        Normalize(recipe);

        await _uow.Repository<Recipe>().AddAsync(recipe);
        await _uow.CompleteAsync();
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        Normalize(recipe);

        _uow.Repository<Recipe>().Update(recipe);
        await _uow.CompleteAsync();
    }

    public async Task DeleteAsync(Recipe recipe)
    {
        recipe.IsDeleted = true;
        _uow.Repository<Recipe>().Update(recipe);
        await _uow.CompleteAsync();
    }

    private static void Normalize(Recipe recipe)
    {
        recipe.Ingredients ??= [];
        recipe.Steps ??= [];
    }
}

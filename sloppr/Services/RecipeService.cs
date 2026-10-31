using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;
using sloppr.Recipes;

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

    public async Task<RecipeSaveResult> AddAsync(Recipe recipe)
    {
        var format = await _db.RecipeFormats
            .Where(f => f.IsCurrent && f.Key == RecipeFormat.Key)
            .OrderByDescending(f => f.Version)
            .FirstOrDefaultAsync();
        if (format is null)
        {
            return new RecipeSaveResult(null, ["No current recipe format is configured."]);
        }

        recipe.FormatKey = format.Key;
        recipe.FormatVersion = format.Version;

        Normalize(recipe);

        var errors = await ValidateAsync(recipe);
        if (errors.Count > 0)
        {
            return new RecipeSaveResult(null, errors);
        }

        await _uow.Repository<Recipe>().AddAsync(recipe);
        await _uow.CompleteAsync();
        return new RecipeSaveResult(recipe, []);
    }

    public async Task<RecipeSaveResult> UpdateAsync(Recipe recipe)
    {
        var errors = new List<string>();

        Normalize(recipe);

        var formatExists = await _db.RecipeFormats
            .AnyAsync(f => f.Key == recipe.FormatKey && f.Version == recipe.FormatVersion);
        if (!formatExists)
        {
            errors.Add("Unknown recipe format.");
        }

        errors.AddRange(await ValidateAsync(recipe));
        if (errors.Count > 0)
        {
            return new RecipeSaveResult(null, errors);
        }

        _uow.Repository<Recipe>().Update(recipe);
        await _uow.CompleteAsync();
        return new RecipeSaveResult(recipe, []);
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

    private async Task<List<string>> ValidateAsync(Recipe recipe)
    {
        var errors = new List<string>();

        if (recipe.MealIdeaId is int mealIdeaId
            && !await _db.MealIdeas.AnyAsync(m => m.Id == mealIdeaId))
        {
            errors.Add("Meal idea does not exist.");
        }

        errors.AddRange(RecipeFormatValidator.Validate(recipe));
        return errors;
    }
}

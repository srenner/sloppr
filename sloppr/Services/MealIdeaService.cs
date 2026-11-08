using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class MealIdeaService : IMealIdeaService
{
    private readonly ApplicationDbContext _db;

    public MealIdeaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MealIdea?> GetByIdAsync(int id)
    {
        return await _db.MealIdeas.FindAsync(id);
    }

    public async Task<MealIdea?> GetByIdWithIngredientsAsync(int id)
    {
        return await _db.MealIdeas
            .Include(m => m.KeyIngredients)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MealIdea>> GetFilteredAsync()
    {
        return await _db.MealIdeas
            .Include(m => m.KeyIngredients)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MealIdea>> GetAllAsync()
    {
        return await _db.MealIdeas
            .IgnoreQueryFilters()
            .Include(m => m.KeyIngredients)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MealIdea> UpdateAsync(MealIdea idea)
    {
        _db.MealIdeas.Update(idea);
        await _db.SaveChangesAsync();
        return idea;
    }

    public async Task<IEnumerable<MealIdea>> CreateRangeAsync(IEnumerable<string> names, IEnumerable<int> keyIngredientIds)
    {
        var ingredients = await _db.KeyIngredients
            .Where(k => keyIngredientIds.Contains(k.Id))
            .ToListAsync();

        var ideas = names
            .Select(name => new MealIdea { Name = name, KeyIngredients = ingredients })
            .ToList();

        _db.MealIdeas.AddRange(ideas);
        await _db.SaveChangesAsync();
        return ideas;
    }
}

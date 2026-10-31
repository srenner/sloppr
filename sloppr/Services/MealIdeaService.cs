using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class MealIdeaService : IMealIdeaService
{
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _db;

    public MealIdeaService(IUnitOfWork uow, ApplicationDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task<MealIdea?> GetByIdAsync(int id)
    {
        return await _uow.Repository<MealIdea>().GetByIdAsync(id);
    }

    public async Task<MealIdea?> GetByIdWithIngredientsAsync(int id)
    {
        return await _uow.Repository<MealIdea>().GetByIdAsync(id, m => m.KeyIngredients);
    }

    public async Task<IEnumerable<MealIdea>> GetFilteredAsync()
    {
        return await _db.MealIdeas
            .Include(m => m.KeyIngredients)
            .ToListAsync();
    }

    public async Task<IEnumerable<MealIdea>> GetAllAsync()
    {
        return await _db.MealIdeas
            .IgnoreQueryFilters()
            .Include(m => m.KeyIngredients)
            .ToListAsync();
    }

    public async Task<MealIdea> UpdateAsync(MealIdea idea)
    {
        _uow.Repository<MealIdea>().Update(idea);
        await _uow.CompleteAsync();
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

        await _uow.Repository<MealIdea>().AddRangeAsync(ideas);
        await _uow.CompleteAsync();
        return ideas;
    }
}

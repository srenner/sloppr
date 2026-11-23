using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Mappers;
using sloppr.Models;

namespace sloppr.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _db;
    private readonly RecipeMapper _mapper;

    public RecipeService(ApplicationDbContext db, RecipeMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        return await _db.Recipes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RecipePageDTO> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Recipes.AsNoTracking();

        var total = await query.CountAsync();
        var recipes = await query
            .OrderByDescending(r => r.DateUpdated)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new RecipePageDTO
        {
            Items = recipes.Select(_mapper.ToSummaryDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
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

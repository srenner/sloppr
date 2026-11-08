using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class AiModelService(ApplicationDbContext db) : IAiModelService
{
    private readonly ApplicationDbContext _db = db;


    #region CRUD operations

    public async Task AddAsync(AiModel model)
    {
        _db.AiModels.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task<int> AddRangeAsync(IEnumerable<AiModel> models)
    {
        _db.AiModels.AddRange(models);
        return await _db.SaveChangesAsync();
    }

    public async Task<AiModel?> GetByIdAsync(int id)
    {
        return await _db.AiModels.FindAsync(id);
    }

    public async Task<AiModel?> GetByIdWithProviderAsync(int id)
    {
        return await _db.AiModels
            .Include(x => x.AiProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<AiModel>> GetFilteredAsync()
    {
        return await _db.AiModels.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<AiModel>> GetAllAsync()
    {
        return await _db.AiModels.IgnoreQueryFilters().AsNoTracking().ToListAsync();
    }

    public async Task<AiModel> UpdateAsync(AiModel model)
    {
        _db.AiModels.Update(model);
        await _db.SaveChangesAsync();
        return model;
    }

    #endregion

}

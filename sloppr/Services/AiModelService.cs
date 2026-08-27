using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class AiModelService(IUnitOfWork uow) : IAiModelService
{
    private readonly IUnitOfWork _uow = uow;

    public async Task AddAsync(AiModel model)
    {
        await _uow.Repository<AiModel>().AddAsync(model);
        await _uow.CompleteAsync();
    }

    public async Task<int> AddRangeAsync(IEnumerable<AiModel> models)
    {
        await _uow.Repository<AiModel>().AddRangeAsync(models);
        return await _uow.CompleteAsync();
    }

    public async Task<IEnumerable<AiModel>> GetFilteredAsync()
    {
        return await _uow.Repository<AiModel>().GetFilteredAsync();
    }

    public async Task<IEnumerable<AiModel>> GetAllAsync()
    {
        return await _uow.Repository<AiModel>().GetAllAsync();
    }

    public async Task<AiModel?> GetByIdAsync(int id)
    {
        return await _uow.Repository<AiModel>().GetByIdAsync(id);
    }

    public async Task<AiModel?> GetByIdWithProviderAsync(int id)
    {
        return await _uow.Repository<AiModel>().GetByIdAsync(id, x => x.AiProvider);
    }

    public async Task<AiModel> UpdateAsync(AiModel model)
    {
        _uow.Repository<AiModel>().Update(model);
        await _uow.CompleteAsync();
        return model;
    }
}

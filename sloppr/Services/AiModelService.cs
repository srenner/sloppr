using System;
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

    public async Task<IEnumerable<AiModel>> GetAllAsync()
    {
        return await _uow.Repository<AiModel>().GetAllAsync();
    }

    public async Task<AiModel?> GetByIdAsync(int id)
    {
        return await _uow.Repository<AiModel>().GetByIdAsync(id);
    }

    public async Task<AiModel> UpdateAsync(AiModel model)
    {
        _uow.Repository<AiModel>().Update(model);
        await _uow.CompleteAsync();
        return model;
    }
}

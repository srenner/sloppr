using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class AiProviderService(IUnitOfWork uow) : IAiProviderService
{
    private readonly IUnitOfWork _uow = uow;

    public async Task AddAsync(AiProvider provider)
    {
        await _uow.Repository<AiProvider>().AddAsync(provider);
        await _uow.CompleteAsync();
    }

    public async Task<IEnumerable<AiProvider>> GetAllAsync()
    {
        return await _uow.Repository<AiProvider>().GetAllAsync();
    }

    public async Task<AiProvider?> GetByIdAsync(int id)
    {
        return await _uow.Repository<AiProvider>().GetByIdAsync(id);
    }

    public async Task<AiProvider> UpdateAsync(AiProvider provider)
    {
        _uow.Repository<AiProvider>().Update(provider);
        await _uow.CompleteAsync();
        return provider;
    }
}

using sloppr.Models;

namespace sloppr.Services;

public interface IAiProviderService
{
    public Task AddAsync(AiProvider provider, bool allowUnhealthy = false);
    public Task<AiProvider?> GetByIdAsync(int id);
    public Task<AiProvider?> GetByIdWithModelsAsync(int id);
    public Task<List<AiModel>> DiscoverModels(int providerId);
    Task<AiProvider?> CheckHealthAsync(int id);
    Task<AiProvider?> CheckHealthAsync(AiProvider provider);
    Task<IEnumerable<AiProvider>> GetFilteredAsync();
    public Task<IEnumerable<AiProvider>> GetAllAsync();
    public Task<AiProvider> UpdateAsync(AiProvider provider);
}

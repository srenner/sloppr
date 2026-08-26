using sloppr.Models;

namespace sloppr.Services;

public interface IAiProviderService
{
    public Task AddAsync(AiProvider provider, bool allowUnhealthy = false);
    public Task<AiProvider?> GetByIdAsync(int id);
    Task<AiProvider?> CheckHealthAsync(int id);
    Task<AiProvider?> CheckHealthAsync(AiProvider provider);
    public Task<IEnumerable<AiProvider>> GetAllAsync();
    public Task<AiProvider> UpdateAsync(AiProvider provider);
}

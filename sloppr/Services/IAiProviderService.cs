using sloppr.Models;

namespace sloppr.Services;

public interface IAiProviderService
{
    public Task AddAsync(AiProvider provider);
    public Task<AiProvider?> GetByIdAsync(int id);
    Task<AiProvider?> CheckHealthAsync(int id);
    public Task<IEnumerable<AiProvider>> GetAllAsync();
    public Task<AiProvider> UpdateAsync(AiProvider provider);
}

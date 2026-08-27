using sloppr.Models;

namespace sloppr.Services;

public interface IAiModelService
{
    public Task AddAsync(AiModel model);
    public Task<int> AddRangeAsync(IEnumerable<AiModel> models);
    public Task<AiModel?> GetByIdAsync(int id);
    public Task<AiModel?> GetByIdWithProviderAsync(int id);
    public Task<IEnumerable<AiModel>> GetFilteredAsync();
    public Task<IEnumerable<AiModel>> GetAllAsync();
    public Task<AiModel> UpdateAsync(AiModel model);
}

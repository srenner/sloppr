using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using sloppr.DataAccess;
using sloppr.Models;
using sloppr.Settings;

namespace sloppr.Services;

public class AiProviderService(ApplicationDbContext db,
                                IHttpClientFactory httpClientFactory,
                                IOptions<ProviderTypeSettings> options,
                                IModelDiscoveryService modelDiscoveryService) : IAiProviderService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ProviderTypeSettings _providerTypeSettings = options.Value;
    private readonly IModelDiscoveryService _modelDiscoveryService = modelDiscoveryService;

    public async Task AddAsync(AiProvider provider, bool allowUnhealthy = false)
    {
        await CheckHealthAsync(provider);
        if (provider.IsHealthy == true || allowUnhealthy)
        {
            _db.AiProviders.Add(provider);
            await _db.SaveChangesAsync();
        }
    }


    public async Task<IEnumerable<AiProvider>> GetFilteredAsync()
    {
        return await _db.AiProviders.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<AiProvider>> GetAllAsync()
    {
        return await _db.AiProviders.IgnoreQueryFilters().AsNoTracking().ToListAsync();
    }

    public async Task<AiProvider?> GetByIdAsync(int id)
    {
        return await _db.AiProviders.FindAsync(id);
    }

    public async Task<AiProvider?> GetByIdWithModelsAsync(int id)
    {
        return await _db.AiProviders
            .Include(x => x.ProviderModels)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AiModel>> DiscoverModels(int providerId)
    {
        var provider = await GetByIdWithModelsAsync(providerId);
        if (provider != null)
        {
            var discoverUrl = provider.BaseUrl + _providerTypeSettings.Types[provider.ProviderType].ModelDiscoveryPath;
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(discoverUrl);
                var responseBody = await response.Content.ReadAsStringAsync();
                var models = _modelDiscoveryService.Parse(provider, responseBody);
                return models;
            }
            catch (Exception ex)
            {
                //
            }
        }
        else
        {
            // no provider, nothing to do
        }
        return new();
    }

    public async Task<AiProvider?> CheckHealthAsync(int id)
    {
        var provider = await GetByIdAsync(id);
        if (provider != null)
        {
            return await CheckHealthAsync(provider);
        }
        else return null;
    }

    public async Task<AiProvider?> CheckHealthAsync(AiProvider provider)
    {
        var healthUrl = provider.BaseUrl + _providerTypeSettings.Types[provider.ProviderType].HealthPath;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("health");
            var response = await httpClient.GetAsync(healthUrl);
            var responseBody = await response.Content.ReadAsStringAsync();

            provider.DateHealthChecked = DateTime.UtcNow;
            provider.IsHealthy = response.IsSuccessStatusCode;
            provider.LastHealthStatusCode = (int?)response.StatusCode;
            provider.LastHealthResponse = responseBody[..Math.Min(200, responseBody.Length)];
        }
        catch (Exception ex)
        {
            provider.DateHealthChecked = DateTime.UtcNow;
            provider.IsHealthy = false;
            provider.LastHealthStatusCode = 0;
            provider.LastHealthResponse = $"Error: {ex.Message}";
        }
        if (provider.Id > 0)
        {
            return await UpdateAsync(provider);
        }
        else
        {
            return provider;
        }
    }

    public async Task<AiProvider> UpdateAsync(AiProvider provider)
    {
        _db.AiProviders.Update(provider);
        await _db.SaveChangesAsync();
        return provider;
    }
}

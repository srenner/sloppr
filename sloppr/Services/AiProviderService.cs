using Microsoft.Extensions.Options;
using sloppr.AI;
using sloppr.AI.ModelDiscoveryResponse;
using sloppr.DataAccess;
using sloppr.Models;
using sloppr.Settings;
using System.Net.Http;
using System.Text;

namespace sloppr.Services;

public class AiProviderService(IUnitOfWork uow,
                                IHttpClientFactory httpClientFactory,
                                IOptions<ProviderTypeSettings> options,
                                IModelDiscoveryService modelDiscoveryService) : IAiProviderService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ProviderTypeSettings _providerTypeSettings = options.Value;
    private readonly IModelDiscoveryService _modelDiscoveryService = modelDiscoveryService;

    public async Task AddAsync(AiProvider provider, bool allowUnhealthy = false)
    {
        await CheckHealthAsync(provider);
        if (provider.IsHealthy == true || allowUnhealthy)
        {
            await _uow.Repository<AiProvider>().AddAsync(provider);
            await _uow.CompleteAsync();
        }
    }


    public async Task<IEnumerable<AiProvider>> GetFilteredAsync()
    {
        return await _uow.Repository<AiProvider>().GetFilteredAsync();
    }

    public async Task<IEnumerable<AiProvider>> GetAllAsync()
    {
        return await _uow.Repository<AiProvider>().GetAllAsync();
    }

    public async Task<AiProvider?> GetByIdAsync(int id)
    {
        return await _uow.Repository<AiProvider>().GetByIdAsync(id);
    }

    public async Task<AiProvider?> GetByIdWithModelsAsync(int id)
    {
        return await _uow.Repository<AiProvider>().GetByIdAsync(id, x => x.ProviderModels);
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
        _uow.Repository<AiProvider>().Update(provider);
        await _uow.CompleteAsync();
        return provider;
    }
}

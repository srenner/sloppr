using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Models;
using sloppr.Settings;

namespace sloppr.Services;

public class AiCapabilitySettingService(IServiceScopeFactory scopeFactory,
                                        IOptions<AISettings> aiSettings)
{
    private readonly AISettings _aiSettings = aiSettings.Value;
    private Dictionary<AiCapabilityType, AiCapabilitySetting> _settings = [];

    public IReadOnlyList<AiCapabilitySetting> GetAll()
    {
        return _settings.Values
            .OrderBy(setting => setting.Capability)
            .ToList();
    }

    public AiCapabilitySetting Get(AiCapabilityType capability)
    {
        return _settings.TryGetValue(capability, out var setting)
            ? setting
            : throw new InvalidOperationException(
                $"No settings found for capability '{capability}'.");
    }

    /// <summary>
    /// Loads the capability rows and fills any missing rows or prompts from
    /// configuration.
    /// </summary>
    public async Task Load()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var settings = await db.AiCapabilitySettings.ToListAsync();

        foreach (var capability in Enum.GetValues<AiCapabilityType>())
        {
            var setting = settings.FirstOrDefault(s => s.Capability == capability);
            if (setting is null)
            {
                setting = new AiCapabilitySetting
                {
                    Capability = capability,
                    SystemPrompt = GetDefaultPrompt(capability)
                };
                db.AiCapabilitySettings.Add(setting);
                settings.Add(setting);
            }
            else if (string.IsNullOrWhiteSpace(setting.SystemPrompt))
            {
                setting.SystemPrompt = GetDefaultPrompt(capability);
            }
        }

        await db.SaveChangesAsync();
        _settings = settings.ToDictionary(setting => setting.Capability);
    }

    public async Task<IReadOnlyList<AiCapabilitySetting>> UpdateAsync(
        IReadOnlyList<AiCapabilitySettingRequest> requests)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var settings = await db.AiCapabilitySettings.ToListAsync();

        foreach (var request in requests)
        {
            var setting = settings.FirstOrDefault(s => s.Capability == request.Capability);
            if (setting is null)
            {
                setting = new AiCapabilitySetting { Capability = request.Capability };
                db.AiCapabilitySettings.Add(setting);
                settings.Add(setting);
            }

            setting.ModelId = request.ModelId;
            setting.SystemPrompt = request.SystemPrompt?.Trim() ?? string.Empty;
        }

        await db.SaveChangesAsync();
        _settings = settings.ToDictionary(setting => setting.Capability);
        return GetAll();
    }

    private string GetDefaultPrompt(AiCapabilityType capability)
    {
        return capability switch
        {
            AiCapabilityType.IngredientExtraction => _aiSettings.DefaultExtractionPrompt,
            AiCapabilityType.IdeaGeneration => _aiSettings.IdeaGenerationPrompt,
            AiCapabilityType.RecipeGeneration => _aiSettings.RecipeGenerationPrompt,
            AiCapabilityType.OcrScanning => _aiSettings.OcrScanningPrompt,
            _ => string.Empty
        };
    }
}

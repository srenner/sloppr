using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public class ApplicationSettingService(IServiceScopeFactory scopeFactory)
{
    public ApplicationSetting Settings { get; private set; } = null!;

    public async Task Load()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Settings = await db.ApplicationSettings.SingleAsync();
    }

    /// <summary>
    /// Persists the model selections and refreshes the in-memory copy so the
    /// singleton callers (ChatService) pick up the change without a restart.
    /// </summary>
    public async Task<ApplicationSetting> UpdateAsync(ApplicationSettingRequest request)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var settings = await db.ApplicationSettings.SingleAsync();
        settings.ExtractionModelId = request.ExtractionModelId;
        settings.IdeaModelId = request.IdeaModelId;
        settings.RecipeModelId = request.RecipeModelId;
        await db.SaveChangesAsync();

        Settings = settings;
        return settings;
    }
}

using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
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
}

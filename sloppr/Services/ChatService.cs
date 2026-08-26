using Microsoft.Extensions.Options;
using sloppr.Enums;
using sloppr.Settings;

namespace sloppr.Services;

public class ChatService
{
    private readonly ProviderTypeSettings _providerTypeSettings;

    public ChatService(IOptions<ProviderTypeSettings> options)
    {
        _providerTypeSettings = options.Value; // unwrap the actual settings object
    }

    public string GetChatPath(AiProviderType type)
    {
        return _providerTypeSettings.Types[type].ChatPath;
    }
}

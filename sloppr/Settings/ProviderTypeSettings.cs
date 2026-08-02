using sloppr.Enums;

namespace sloppr.Settings;

public class ProviderTypeSettings
{
    public Dictionary<AiProviderType, ProviderTypeConfig> Types { get; set; } = new();
}

public class ProviderTypeConfig
{
    public string ChatEndpointPath { get; set; } = string.Empty;
}

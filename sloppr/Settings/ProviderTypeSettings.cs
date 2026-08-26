using sloppr.Enums;

namespace sloppr.Settings;

public class ProviderTypeSettings
{
    public Dictionary<AiProviderType, ProviderTypeConfig> Types { get; set; } = new();
}

public class ProviderTypeConfig
{
    public string HealthPath { get; set; } = string.Empty;
    public string ModelDiscoveryPath { get; set; } = string.Empty;
    public string ChatPath { get; set; } = string.Empty;
}

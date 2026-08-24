using sloppr.Enums;

namespace sloppr.AI.DTOs;

public class ChatClientConfig
{
    public AiProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = "";
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
}

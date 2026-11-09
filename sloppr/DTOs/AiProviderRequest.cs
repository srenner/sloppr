using sloppr.Enums;

namespace sloppr.DTOs;

public class AiProviderRequest
{
    public required string Name { get; set; }
    public AiProviderType ProviderType { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

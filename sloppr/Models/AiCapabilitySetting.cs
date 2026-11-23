using sloppr.Enums;

namespace sloppr.Models;

public class AiCapabilitySetting
{
    public int Id { get; set; }
    public AiCapabilityType Capability { get; set; }
    public int? ModelId { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
}

using sloppr.Enums;

namespace sloppr.DTOs;

public class AiCapabilitySettingDTO
{
    public required AiCapabilityType Capability { get; set; }
    public int? ModelId { get; set; }
    public required string SystemPrompt { get; set; }
}

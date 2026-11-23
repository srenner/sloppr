using System.ComponentModel.DataAnnotations;
using sloppr.Enums;

namespace sloppr.DTOs;

public class AiCapabilitySettingRequest
{
    public required AiCapabilityType Capability { get; set; }
    public int? ModelId { get; set; }

    [Required]
    [MaxLength(100_000)]
    public required string SystemPrompt { get; set; }
}

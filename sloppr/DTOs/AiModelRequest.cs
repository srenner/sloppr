namespace sloppr.DTOs;

public class AiModelRequest
{
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public int AiProviderId { get; set; }
    public int? ContextWindow { get; set; }
    public string? Capabilities { get; set; }
    public bool IsActive { get; set; } = true;
}

namespace sloppr.DTOs;

public class AiModelDTO
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = "system";
    public string UpdatedBy { get; set; } = "system";

    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public int AiProviderId { get; set; }
    public required AiProviderDTO AiProvider { get; set; }
    public int? ContextWindow { get; set; }
    public string? Capabilities { get; set; }
}

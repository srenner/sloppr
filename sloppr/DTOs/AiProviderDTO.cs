using sloppr.Enums;

namespace sloppr.DTOs;

public class AiProviderDTO
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = "system";
    public string UpdatedBy { get; set; } = "system";

    public string Name { get; set; } = String.Empty;
    public AiProviderType ProviderType { get; set; }
    public string? BaseUrl { get; set; }
    public string? HealthCheckPath { get; set; }
    public string? ModelDiscoveryPath { get; set; }
    public bool? IsHealthy { get; set; }
    public int? LastHealthStatusCode { get; set; }
    public string? LastHealthResponse { get; set; }
    public DateTime? DateHealthChecked { get; set; }
}


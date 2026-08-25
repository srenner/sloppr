using sloppr.Enums;

namespace sloppr.Models;

public class AiProvider : BaseModel
{
    public required string Name { get; set; }

    public AiProviderType ProviderType { get; set; }

    public string? BaseUrl { get; set; }

    public string? HealthCheckPath { get; set; }

    public bool? IsHealthy { get; set; }

    public int? LastHealthStatusCode { get; set; }
    public string? LastHealthResponse { get; set; }

    public DateTime? DateHealthChecked { get; set; }

    public IEnumerable<AiModel>? ProviderModels { get; set; }
}

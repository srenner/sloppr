using sloppr.Enums;

namespace sloppr.Models;

public class AiProvider : BaseModel
{
    public string Name { get; set; } = String.Empty;

    public AiProviderType ProviderType { get; set; }

    public string? BaseUrl { get; set; }

    public bool? IsHealthy { get; set; }

    public int? LastHealthStatusCode { get; set; }
    public string? LastHealthResponse { get; set; }

    public DateTime? DateHealthChecked { get; set; }

    public ICollection<AiModel> ProviderModels { get; set; } = new List<AiModel>();
}

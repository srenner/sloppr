using Microsoft.Identity.Client;
using sloppr.Enums;

namespace sloppr.Models;

public class AiProvider : BaseModel
{
    public required string Name { get; set; }

    public AiProviderType ProviderType { get; set; }

    public string? BaseUrl { get; set; }

    public IEnumerable<AiModel>? ProviderModels { get; set; }
}

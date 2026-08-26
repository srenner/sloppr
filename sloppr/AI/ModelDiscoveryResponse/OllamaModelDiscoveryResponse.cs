using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace sloppr.AI.ModelDiscoveryResponse;

public class OllamaModelDiscoveryResponse : IModelDiscoveryResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<ModelInfo> Data { get; set; } = new List<ModelInfo>();
}

public class ModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; } = "model";

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; }
}
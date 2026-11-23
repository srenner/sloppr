using System.Text.Json.Serialization;

namespace sloppr.DTOs;

public class ApplicationSettingDTO
{
    public int? ExtractionModelId { get; set; }
    public int? IdeaModelId { get; set; }
    public int? RecipeModelId { get; set; }

    [JsonPropertyName("ocrModelId")]
    public int? OCRModelId { get; set; }
}

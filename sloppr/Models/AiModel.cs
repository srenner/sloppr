using System.Text.Json;
using sloppr.Enums;

namespace sloppr.Models;

public class AiModel : BaseModel
{
    /// <summary>Friendly name</summary>
    public required string Name { get; set; }

    /// <summary>Name the API uses (e.g. "granite4.1:8b" instead of "Granite 4.1")</summary>
    public required string Identifier { get; set; }

    public int AiProviderId { get; set; }

    public AiProvider? AiProvider { get; set; }

    public int? ContextWindow { get; set; }

    /// <summary>
    /// Simple Json string. SQLite cannot handle JsonDocument natively.
    /// </summary>
    public string? Capabilities { get; set; }
}

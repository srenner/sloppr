using Scalar.AspNetCore.Attributes;
using sloppr.Enums;

namespace sloppr.AI.DTOs;

[Deprecated]
public class ChatApiResponse
{
    public ChatResult Result { get; set; } = new();
    public AiProviderType Provider { get; set; }
    public string? Model { get; set; }
    public int? InputTokenCount { get; set; }
    public int? OutputTokenCount { get; set; }

    #region Ollama

    /// <remarks>Ollama-specific</remarks>
    public long? EvalCount { get; set; }

    /// <remarks>Ollama-specific</remarks>
    public long? TotalDurationNs { get; set; }

    /// <remarks>Ollama-specific</remarks>
    public string? DoneReason { get; set; }

    #endregion

}

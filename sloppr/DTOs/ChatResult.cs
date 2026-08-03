namespace sloppr.DTOs;

public class ChatResult
{
    public string Answer { get; set; } = "";

    public string? Reasoning { get; set; }

    public double? Confidence { get; set; }
}

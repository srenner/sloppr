namespace sloppr.DTOs;

public class AiProviderHealthDTO
{
    public int Id { get; set; }
    public bool? IsHealthy { get; set; }
    public string? LastHealthResponse { get; set; }
    public DateTime? DateHealthChecked { get; set; }
}

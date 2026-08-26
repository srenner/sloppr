namespace sloppr.DTOs;

public class AiProviderHealthRequest
{
    public required string BaseUrl { get; set; }
    public required string HealthCheckPath { get; set; }
}

using sloppr.Models;

namespace sloppr.Services;

public interface IModelDiscoveryService
{
    List<AiModel> Parse(AiProvider provider, string response);
}

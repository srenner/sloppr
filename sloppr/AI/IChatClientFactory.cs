using Microsoft.Extensions.AI;
using sloppr.AI.DTOs;

namespace sloppr.AI;

public interface IChatClientFactory
{
    IChatClient Create(ChatClientConfig config);
}

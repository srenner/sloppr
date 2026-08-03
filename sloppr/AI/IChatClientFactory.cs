using System;
using Microsoft.Extensions.AI;
using sloppr.Enums;

namespace sloppr.AI;

public interface IChatClientFactory
{
    IChatClient Create(AiProviderType provider, string model);
}

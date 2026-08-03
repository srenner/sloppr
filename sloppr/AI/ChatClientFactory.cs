using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI.Chat;
using sloppr.Enums;

namespace sloppr.AI;

public class ChatClientFactory : IChatClientFactory
{
    private readonly IConfiguration _config;
    private readonly Dictionary<AiProviderType, Func<string, IChatClient>> _providers;

    public ChatClientFactory(IConfiguration config)
    {
        _config = config;

        _providers = new()
        {
            [AiProviderType.OpenAI] = CreateOpenAiClient,
            [AiProviderType.Ollama] = CreateOllamaClient
        };
    }

    public IChatClient Create(AiProviderType provider, string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model must be specified.", nameof(model));

        if (!_providers.TryGetValue(provider, out var factory))
            throw new NotSupportedException($"Unknown provider: '{provider}'.");

        return factory(model);
    }

    private IChatClient CreateOpenAiClient(string model)
    {
        var apiKey = _config["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        return new ChatClient(model, apiKey).AsIChatClient();
    }

    private IChatClient CreateOllamaClient(string model)
    {
        var endpoint = _config["Ollama:Endpoint"]
            ?? throw new InvalidOperationException("Ollama:Endpoint is not configured.");

        return new OllamaApiClient(new Uri(endpoint), model);
    }
}
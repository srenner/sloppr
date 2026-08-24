using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI.Chat;
using sloppr.AI.DTOs;
using sloppr.Enums;

namespace sloppr.AI;

public class ChatClientFactory : IChatClientFactory
{
    public IChatClient Create(ChatClientConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ModelName))
            throw new ArgumentException("ModelName must be specified.", nameof(config));

        return config.ProviderType switch
        {
            AiProviderType.OpenAI => CreateOpenAiClient(config),
            AiProviderType.Ollama => CreateOllamaClient(config),
            _ => throw new NotSupportedException($"Unknown provider type: '{config.ProviderType}'.")
        };
    }

    private static IChatClient CreateOpenAiClient(ChatClientConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("OpenAI config is missing an API key.");

        return new ChatClient(config.ModelName, config.ApiKey).AsIChatClient();
    }

    private static IChatClient CreateOllamaClient(ChatClientConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Endpoint))
            throw new InvalidOperationException("Ollama config is missing an endpoint.");

        return new OllamaApiClient(new Uri(config.Endpoint), config.ModelName);
    }
}

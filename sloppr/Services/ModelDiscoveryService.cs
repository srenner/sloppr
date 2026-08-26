using System.Text.Json;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using sloppr.AI.ModelDiscoveryResponse;
using sloppr.Enums;
using sloppr.Models;

namespace sloppr.Services;

public class ModelDiscoveryService : IModelDiscoveryService
{
    public List<AiModel> Parse(AiProvider provider, string response)
    {

        var discoveredModels =
        provider.ProviderType switch
        {
            AiProviderType.Ollama => ParseOllama(response),
            AiProviderType.OpenAI => ParseOpenAI(response),
            _ => throw new NotSupportedException($"Unknown provider type: '{provider.ProviderType}'.")
        };

        var existingModelIdentifiers = provider.ProviderModels.Select(m => m.Identifier).ToHashSet();
        return discoveredModels.Where(model => !existingModelIdentifiers.Contains(model.Identifier)).ToList();
    }

    private static List<AiModel> ParseOllama(string response)
    {
        var models = new List<AiModel>();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaModelDiscoveryResponse>(response);

        if (ollamaResponse?.Data?.Count > 0)
        {
            foreach (var model in ollamaResponse.Data)
            {
                models.Add(new AiModel
                {
                    Identifier = model.Id,
                    Name = model.Id,
                    AiProvider = new()
                });
            }
        }
        return models;
    }

    private static List<AiModel> ParseOpenAI(string response)
    {
        throw new NotImplementedException();
    }
}

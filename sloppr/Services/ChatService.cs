using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.Content;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Mappers;
using sloppr.Models;
using sloppr.Settings;

namespace sloppr.Services;

public class ChatService
{
    private readonly ProviderTypeSettings _providerTypeSettings;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IAiModelService _modelService;
    private readonly IChatClientFactory _factory;
    private readonly AiCapabilitySettingService _capabilitySettings;
    private readonly RecipeMapper _recipeMapper;

    public ChatService(IOptions<ProviderTypeSettings> options,
                        IAiModelService modelService,
                        IChatClientFactory factory,
                        AiCapabilitySettingService capabilitySettings,
                        RecipeMapper recipeMapper)
    {
        _providerTypeSettings = options.Value; // unwrap the actual settings object
        _modelService = modelService;
        _factory = factory;
        _capabilitySettings = capabilitySettings;
        _recipeMapper = recipeMapper;
    }

    public string GetChatPath(AiProviderType type)
    {
        return _providerTypeSettings.Types[type].ChatPath;
    }

    public async Task<(string[] Ingredients, string[] Cuisines)> ExtractIngredientsAndCuisines(string prompt)
    {
        var capability = _capabilitySettings.Get(AiCapabilityType.IngredientExtraction);
        if (capability.ModelId is not int modelId)
        {
            return ([], []);
        }

        var model = await _modelService.GetByIdWithProviderAsync(modelId);
        if (model is null)
        {
            return ([], []);
        }

        var extracted = await ExtractResponse<ExtractedIngredientsAndCuisinesResponse>(
            capability.SystemPrompt, prompt, model);

        return extracted is null
            ? ([], [])
            : (extracted.Ingredients.ToArray(), extracted.Cuisines.ToArray());
    }

    private async Task<TResponse?> ExtractResponse<TResponse>(string systemPrompt,
                                                              string userPrompt,
                                                              AiModel model)
        where TResponse : class
    {
        var config = new ChatClientConfig
        {
            ProviderType = model.AiProvider.ProviderType,
            ModelName = model.Identifier,
            Endpoint = model.AiProvider.BaseUrl,
            ApiKey = null // todo,
        };

        IChatClient client = _factory.Create(config);
        List<ChatMessage> messages = new()
        {
            new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt),
        };

        ChatResponse response = await client.GetResponseAsync(messages, new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<TResponse>(JsonOptions)
        });

        return JsonSerializer.Deserialize<TResponse>(response.Text, JsonOptions);
    }

    public async Task<string[]> GenerateIdeas(string userPrompt, string[] keyIngredients, string[] cuisines)
    {
        var capability = _capabilitySettings.Get(AiCapabilityType.IdeaGeneration);
        if (capability.ModelId is not int modelId)
        {
            return [];
        }

        var model = await _modelService.GetByIdWithProviderAsync(modelId);
        if (model is null)
        {
            return [];
        }

        var config = new ChatClientConfig
        {
            ProviderType = model.AiProvider.ProviderType,
            ModelName = model.Identifier,
            Endpoint = model.AiProvider.BaseUrl,
            ApiKey = null // todo
        };

        IChatClient client = _factory.Create(config);

        StringBuilder refinedPrompt = new StringBuilder("Plan a meal");
        if (keyIngredients.Length > 0)
        {
            refinedPrompt.Append(" based on the following ingredients: ");
            refinedPrompt.Append(String.Join(", ", keyIngredients));

        }
        if (cuisines.Length > 0)
        {
            refinedPrompt.Append(". User has cuisine preference: ");
            refinedPrompt.Append(string.Join(", ", cuisines) + ".");
        }
        if (userPrompt.Length > 0)
        {
            refinedPrompt.Append(" Input from user: ");
            refinedPrompt.Append(userPrompt);
        }

        List<ChatMessage> messages = new()
            {
                new ChatMessage(ChatRole.System, capability.SystemPrompt),
                new ChatMessage(ChatRole.User, refinedPrompt.ToString()),
            };
        ChatResponse response = await client.GetResponseAsync(messages, new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<GeneratedIdeasResponse>(JsonOptions)
        });

        var generated = JsonSerializer.Deserialize<GeneratedIdeasResponse>(response.Text, JsonOptions);
        return generated is null ? [] : generated.Ideas.ToArray();
    }

    public async Task<Recipe?> GenerateRecipeAsync(MealIdea idea)
    {
        var capability = _capabilitySettings.Get(AiCapabilityType.RecipeGeneration);
        if (capability.ModelId is not int recipeModelId)
        {
            return null;
        }

        var model = await _modelService.GetByIdWithProviderAsync(recipeModelId);
        if (model == null)
        {
            return null;
        }

        var config = new ChatClientConfig
        {
            ProviderType = model.AiProvider.ProviderType,
            ModelName = model.Identifier,
            Endpoint = model.AiProvider.BaseUrl,
            ApiKey = null // todo
        };

        var keyIngredients = string.Join(", ", idea.KeyIngredients.Select(k => k.Name));
        var userPrompt = $"Meal idea: {idea.Name}\n"
            + $"Key ingredients: {keyIngredients}";

        List<ChatMessage> messages = new()
        {
            new ChatMessage(ChatRole.System, capability.SystemPrompt),
            new ChatMessage(ChatRole.User, userPrompt),
        };

        try
        {
            IChatClient client = _factory.Create(config);
            ChatResponse response = await client.GetResponseAsync(
                messages, new ChatOptions
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<RecipeContent>(
                        JsonOptions, "recipe", "A complete recipe")
                });

            var generated = JsonSerializer.Deserialize<RecipeContent>(
                response.Text, JsonOptions);
            if (generated == null)
            {
                return null;
            }

            generated.Steps = NormalizeSteps(generated.Steps);

            return _recipeMapper.ToEntity(generated);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Some models emit a step whose only content is a section label. Treat that as
    /// the group for the steps that follow and drop the empty step so the rendered
    /// recipe does not contain blank numbered entries.
    /// </summary>
    private static List<RecipeStepContent> NormalizeSteps(List<RecipeStepContent>? steps)
    {
        var normalized = new List<RecipeStepContent>();
        string? group = null;

        foreach (var step in steps ?? [])
        {
            if (string.IsNullOrWhiteSpace(step.Instruction))
            {
                if (!string.IsNullOrWhiteSpace(step.Group))
                {
                    group = step.Group.Trim();
                }
                continue;
            }

            step.Instruction = step.Instruction.Trim();
            step.Group = string.IsNullOrWhiteSpace(step.Group) ? group : step.Group.Trim();
            normalized.Add(step);
        }

        return normalized;
    }
}



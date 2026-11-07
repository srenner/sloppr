using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Models;
using sloppr.Settings;

namespace sloppr.Services;

public class ChatService
{
    private readonly ProviderTypeSettings _providerTypeSettings;
    private readonly AISettings _aiSettings;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IAiModelService _modelService;
    private readonly IChatClientFactory _factory;
    private readonly ApplicationSettingService _appSettingService;
    private readonly RecipeMapper _recipeMapper;

    public ChatService(IOptions<ProviderTypeSettings> options,
                        IOptions<AISettings> aiSettings,
                        IAiModelService modelService,
                        IChatClientFactory factory,
                        ApplicationSettingService appSettingService,
                        RecipeMapper recipeMapper)
    {
        _providerTypeSettings = options.Value; // unwrap the actual settings object
        _aiSettings = aiSettings.Value;
        _modelService = modelService;
        _factory = factory;
        _appSettingService = appSettingService;
        _recipeMapper = recipeMapper;
    }

    public string GetChatPath(AiProviderType type)
    {
        return _providerTypeSettings.Types[type].ChatPath;
    }

    public async Task<(string[] Ingredients, string[] Cuisines)> ExtractIngredientsAndCuisines(string prompt)
    {
        var model = await _modelService.GetByIdWithProviderAsync(_appSettingService.Settings.ExtractionModelId.Value);
        if (model is null)
        {
            return ([], []);
        }

        var extracted = await ExtractResponse<ExtractedIngredientsAndCuisinesResponse>(
            _aiSettings.DefaultExtractionPrompt, prompt, model);

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
        var model = await _modelService.GetByIdWithProviderAsync(_appSettingService.Settings.IdeaModelId.Value);
        if (model != null)
        {
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
                    new ChatMessage(ChatRole.System, _aiSettings.IdeaGenerationPrompt),
                    new ChatMessage(ChatRole.User, refinedPrompt.ToString()),
                };
            ChatResponse response = await client.GetResponseAsync(messages, new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<GeneratedIdeasResponse>(JsonOptions)
            });

            var generated = JsonSerializer.Deserialize<GeneratedIdeasResponse>(response.Text, JsonOptions);
            return generated is null ? [] : generated.Ideas.ToArray();
        }
        else
        {
            return [];
        }
    }

    public async Task<Recipe?> GenerateRecipeAsync(MealIdea idea)
    {
        if (_appSettingService.Settings.RecipeModelId is not int recipeModelId)
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
            new ChatMessage(ChatRole.System, _aiSettings.RecipeGenerationPrompt),
            new ChatMessage(ChatRole.User, userPrompt),
        };

        try
        {
            IChatClient client = _factory.Create(config);
            ChatResponse response = await client.GetResponseAsync(
                messages, new ChatOptions
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<RecipeGenerationResponse>(
                        JsonOptions, "recipe", "A complete recipe")
                });

            var generated = JsonSerializer.Deserialize<RecipeGenerationResponse>(
                response.Text, JsonOptions);
            if (generated == null)
            {
                return null;
            }

            var recipe = _recipeMapper.ToEntity(generated);

            var ingredientOrder = 1;
            foreach (var ingredient in recipe.Ingredients)
            {
                ingredient.Order = ingredientOrder++;
            }

            var stepOrder = 1;
            foreach (var step in recipe.Steps)
            {
                step.Order = stepOrder++;
            }

            return recipe;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

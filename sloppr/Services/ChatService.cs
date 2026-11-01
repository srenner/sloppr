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
using sloppr.Utilities;

namespace sloppr.Services;

public class ChatService
{
    private readonly ProviderTypeSettings _providerTypeSettings;
    private readonly AISettings _aiSettings;
    private static readonly JsonSerializerOptions RecipeJsonOptions = new()
    {
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

    public async Task<string> ExecuteIngredientChallengeAsync(int modelId)
    {
        var model = await _modelService.GetByIdWithProviderAsync(modelId);


        var systemPrompt = _aiSettings.DefaultIngredientExtractionPrompt;
        var challenges = _aiSettings.ExtractionChallenges;

        var config = new ChatClientConfig
        {
            ProviderType = model.AiProvider.ProviderType,
            ModelName = model.Identifier,
            Endpoint = model.AiProvider.BaseUrl,
            ApiKey = null // todo
        };

        return "";
    }


    public async Task ExecuteOrchestration(string userPrompt)
    {
        // extract ingredients from prompt
        // system prompt specifies LLM to return json array of key ingredients
        // Upsert to KeyIngredients table, increment numqueried
        string[] keyIngredients = ["ground beef", "rice", "carrots"];

        // generate 3 ideas from keyIngredients
        // system prompt specifies LLM to return json array of ideas
        string[] mealIdeas =
        {
            "Beef & Carrot Fried Rice",
            "Korean-Style Beef Rice Bowls",
            "Stuffed Bell Peppers with Beef & Rice"
        };

        // user can thumbs-down an idea they don't want to see
        // user picks idea to generate recipe
        // LLM returns markdown recipe

        // recipe builder prompt:

        string rPrompt = "Generate a recipe for " + mealIdeas[0] +
        " for a family dinner. The meal should incorporate the following key ingredients: "
        + string.Join(", ", keyIngredients) + ". Additional ingredients should only contain common household staples.";


    }

    public async Task<string[]> ExtractIngredients(string prompt)
    {
        var model = await _modelService.GetByIdWithProviderAsync(_appSettingService.Settings.ExtractionModelId.Value);
        if (model != null)
        {
            var systemPrompt = _aiSettings.DefaultIngredientExtractionPrompt;
            return await ExtractArray(systemPrompt, prompt, model);
        }
        else
        {
            return [];
        }
    }

    public async Task<string[]> ExtractCuisines(string prompt)
    {
        var model = await _modelService.GetByIdWithProviderAsync(_appSettingService.Settings.ExtractionModelId.Value);
        if (model != null)
        {
            var systemPrompt = _aiSettings.DefaultCuisineExtractionPrompt;
            return await ExtractArray(systemPrompt, prompt, model);
        }
        else
        {
            return [];
        }
    }

    private async Task<string[]> ExtractArray(string systemPrompt, string userPrompt, AiModel model)
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

        ChatResponse? response = await client.GetResponseAsync(messages, new ChatOptions { ResponseFormat = ChatResponseFormat.Json });
        return LlmArrayResponse.Read(response.Text).ToArray();
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

            List<ChatMessage> messages = new()
                {
                    new ChatMessage(ChatRole.System, _aiSettings.IdeaGenerationPrompt),
                    new ChatMessage(ChatRole.User, refinedPrompt.ToString()),
                };
            ChatResponse? response = await client.GetResponseAsync(messages, new ChatOptions { ResponseFormat = ChatResponseFormat.Json });
            return LlmArrayResponse.Read(response.Text).ToArray();
        }
        else
        {
            return [];
        }
    }

    public async Task<Recipe?> GenerateRecipeAsync(MealIdea idea, string formatRules)
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
            + $"Key ingredients: {keyIngredients}\n"
            + $"Format rules (JSON):\n{formatRules}";

        List<ChatMessage> messages = new()
        {
            new ChatMessage(ChatRole.System, _aiSettings.RecipeGenerationPrompt),
            new ChatMessage(ChatRole.User, userPrompt),
        };

        try
        {
            IChatClient client = _factory.Create(config);
            ChatResponse? response = await client.GetResponseAsync(
                messages, new ChatOptions { ResponseFormat = ChatResponseFormat.Json });

            var generated = JsonSerializer.Deserialize<RecipeGenerationResponse>(
                response.Text, RecipeJsonOptions);

            return generated == null ? null : _recipeMapper.ToEntity(generated);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

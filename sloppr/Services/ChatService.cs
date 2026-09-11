using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.Enums;
using sloppr.Settings;

namespace sloppr.Services;

public class ChatService
{
    private readonly ProviderTypeSettings _providerTypeSettings;
    private readonly AISettings _aiSettings;
    private readonly IAiModelService _modelService;
    private readonly IChatClientFactory _factory;
    private readonly ApplicationSettingService _appSettingService;

    public ChatService(IOptions<ProviderTypeSettings> options,
                        IOptions<AISettings> aiSettings,
                        IAiModelService modelService,
                        IChatClientFactory factory,
                        ApplicationSettingService appSettingService)
    {
        _providerTypeSettings = options.Value; // unwrap the actual settings object
        _aiSettings = aiSettings.Value;
        _modelService = modelService;
        _factory = factory;
        _appSettingService = appSettingService;
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
            var ingredientPrompt = _aiSettings.DefaultIngredientExtractionPrompt;
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
                    new ChatMessage(ChatRole.System, ingredientPrompt),
                    new ChatMessage(ChatRole.User, prompt),
                };
            ChatResponse? response = await client.GetResponseAsync(messages, new ChatOptions { ResponseFormat = ChatResponseFormat.Json });
            return JsonSerializer.Deserialize<string[]>(response.Text);
        }
        else
        {
            return [];
        }
    }

    public async Task<string[]> GenerateIdeas(string userPrompt, string[] keyIngredients)
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

            List<ChatMessage> messages = new()
                {
                    new ChatMessage(ChatRole.System, _aiSettings.IdeaGenerationPrompt),
                    new ChatMessage(ChatRole.User, string.Join(", ", keyIngredients)),
                };
            ChatResponse? response = await client.GetResponseAsync(messages, new ChatOptions { ResponseFormat = ChatResponseFormat.Json });
            return JsonSerializer.Deserialize<string[]>(response.Text);
        }
        else
        {
            return [];
        }
    }
}

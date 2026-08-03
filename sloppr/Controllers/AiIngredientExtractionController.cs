using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using sloppr.DTOs;
using sloppr.Services;
using sloppr.Settings;
using Microsoft.Extensions.AI;
using OllamaSharp;
using sloppr.AI.DTOs;
using sloppr.AI;
using OpenAI.Assistants;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiIngredientExtractionController(IAiModelService modelService,
        IOptions<ProviderTypeSettings> providerTypeOptions,
        IOptions<AISettings> aiSettings,
        IChatClientFactory factory)
        : ControllerBase
    {
        [HttpGet("challenge")]
        public async Task<IActionResult> ExecuteIngredientExtractionAsync(int modelId)
        {
            var model = await modelService.GetByIdWithProviderAsync(modelId);

            if (model == null) return NotFound();

            var baseUrl = model.AiProvider.BaseUrl;
            var chatUrl = providerTypeOptions.Value.Types[model.AiProvider.ProviderType].ChatEndpointPath;
            var endpoint = baseUrl + chatUrl;

            var systemPrompt = aiSettings.Value.DefaultIngredientExtractionPrompt;
            var challenges = aiSettings.Value.ExtractionChallenges;

            var config = new ChatClientConfig
            {
                ProviderType = model.AiProvider.ProviderType,
                ModelName = model.Identifier,
                Endpoint = model.AiProvider.BaseUrl,
                ApiKey = null // todo
            };

            IChatClient client = factory.Create(config);

            foreach (var challenge in challenges)
            {

                List<ChatMessage> messages = new()
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, challenge.Prompt),
                };
                var response = await client.GetResponseAsync(messages);

                // TODO: Persist or process the result as needed
            }
            return Ok();
        }
    }
}


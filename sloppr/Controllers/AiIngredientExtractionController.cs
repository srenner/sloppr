using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using sloppr.Services;
using sloppr.Settings;
using Microsoft.Extensions.AI;
using sloppr.AI.DTOs;
using sloppr.AI;
using sloppr.DTOs;

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
        public async Task<ActionResult> ExecuteIngredientExtractionAsync(int modelId)
        {
            var model = await modelService.GetByIdWithProviderAsync(modelId);

            if (model == null) return NotFound();

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

            var dto = new IngredientExtractionResponse(model, systemPrompt);

            foreach (var challenge in challenges)
            {
                List<ChatMessage> messages = new()
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, challenge.Prompt),
                };
                ChatResponse? response = await client.GetResponseAsync(messages);
                dto.Challenges.Add(new IngredientChallenge(challenge.Prompt, challenge.ExpectedResponse,
                                        response.Text, response.Usage.InputTokenCount, response.Usage.OutputTokenCount));
            }
            return Ok(dto);
        }
    }
}

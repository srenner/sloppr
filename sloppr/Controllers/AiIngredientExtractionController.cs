using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using sloppr.DTOs;
using sloppr.Services;
using sloppr.Settings;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiIngredientExtractionController(IAiModelService modelService,
        IOptions<ProviderTypeSettings> providerTypeOptions,
        IOptions<AISettings> aiSettings)
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

            foreach (var challenge in challenges)
            {
                var requestBody = new
                {
                    model = model.Identifier,
                    stream = false,
                    messages = new List<object>
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = challenge.Prompt }
                    }
                };

                using var httpClient = new HttpClient();

                var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama error {response.StatusCode}: {result}");
                }

                //IChatClient chatClient = new OllamaSharp.OllamaApiClient()
                IChatClient client = new OllamaApiClient(new Uri(baseUrl!), model.Identifier);
                Console.WriteLine(await client.GetResponseAsync("What is AI?"));
                var x = await client.GetResponseAsync<OllamaResponse>(challenge.Prompt);


                // TODO: Persist or process the result as needed
            }
            return Ok();
        }
    }
}


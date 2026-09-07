using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.Enums;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ChatService chatService, IAiModelService modelService, IChatClientFactory factory) : ControllerBase
    {
        /// <remarks>
        /// TEMPORARY — for local testing only. Not part of core functionality. Safe to delete.
        /// </remarks>
        [HttpPost("chat")]
        public IActionResult SendChat(AiProviderType type, string prompt)
        {
            var path = chatService.GetChatPath(type);
            // build full URL, call Ollama, etc.
            return Ok();
        }


        [HttpPost("recipe")]
        public async Task<ActionResult> PostRecipeRequest([FromQuery] List<string> ingredients)
        {

            // get default model for recipe generation

            var model = await modelService.GetByIdWithProviderAsync(11);

            if (model != null)
            {
                var config = new ChatClientConfig
                {
                    ProviderType = model.AiProvider.ProviderType,
                    ModelName = model.Identifier,
                    Endpoint = model.AiProvider.BaseUrl
                };


                string systemPrompt = "You are a family meal planner who takes a list of key ingredients and suggests 3 options for what to make for dinner with those ingredients. Respond with a json array of strings. Do not add any additional text or notation aside from the json array of strings.";

                IChatClient client = factory.Create(config);
                List<ChatMessage> messages = new()
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, string.Join(", ", ingredients)),
                };
                ChatResponse? response = await client.GetResponseAsync(messages);
                return Ok(response);
            }
            return Ok();
        }

        [HttpGet("idea")]
        public async Task<ActionResult> GetIdeas(string prompt)
        {
            var ingredients = await chatService.ExtractIngredients(prompt);
            var ideas = await chatService.GenerateIdeas(prompt, ingredients);
            return Ok(ideas);
        }
    }
}

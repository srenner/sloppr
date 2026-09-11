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
    public class ChatController(ChatService chatService,
                                IAiModelService modelService,
                                IChatClientFactory factory,
                                IKeyIngredientService keyIngredientService) : ControllerBase
    {
        /// <remarks>
        /// TEMPORARY — for local testing only. Not part of core functionality. Safe to delete.
        /// </remarks>
        [HttpPost("path")]
        public IActionResult SendChat(AiProviderType type, string prompt)
        {
            var path = chatService.GetChatPath(type);
            // build full URL, call Ollama, etc.
            return Ok();
        }

        [HttpPost("idea")]
        public async Task<ActionResult> GetIdeas(string prompt)
        {
            var ingredients = await chatService.ExtractIngredients(prompt);

            var ingredientDTOs = await keyIngredientService.Upsert(ingredients);

            var ideas = await chatService.GenerateIdeas(prompt, ingredients);
            return Ok(ideas);
        }
    }
}

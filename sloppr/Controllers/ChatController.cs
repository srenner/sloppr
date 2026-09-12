using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ChatService chatService,
                                IAiModelService modelService,
                                IChatClientFactory factory,
                                IKeyIngredientService keyIngredientService,
                                ICuisineService cuisineService) : ControllerBase
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
            await keyIngredientService.Upsert(ingredients);

            var cuisines = await chatService.ExtractCuisines(prompt);
            await cuisineService.Upsert(cuisines);

            var ideas = await chatService.GenerateIdeas(prompt, ingredients);
            var dtos = new List<MealIdeaDTO>();

            return Ok(ideas);
        }
    }
}

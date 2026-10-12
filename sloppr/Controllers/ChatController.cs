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

        [HttpPost("idea")]
        public async Task<ActionResult> GetIdeas(string prompt)
        {
            var ingredients = await chatService.ExtractIngredients(prompt);
            await keyIngredientService.Upsert(ingredients);

            var cuisines = await chatService.ExtractCuisines(prompt);
            await cuisineService.Upsert(cuisines);

            var ideas = await chatService.GenerateIdeas(prompt, ingredients, cuisines);
            var dtos = new List<MealIdeaDTO>();

            return Ok(ideas);
        }
    }
}

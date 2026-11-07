using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiInferenceController(ChatService chatService,
                                       IKeyIngredientService keyIngredientService,
                                       ICuisineService cuisineService,
                                       IMealIdeaService mealIdeaService,
                                       MealIdeaMapper mealIdeaMapper,
                                       RecipeMapper recipeMapper) : ControllerBase
    {
        /// <summary>
        /// Take initial user prompt, upsert extracted ingredients and cuisines, and generate meal ideas.
        /// </summary>
        [HttpPost("meal-ideas")]
        public async Task<ActionResult<IEnumerable<MealIdeaDTO>>> GenerateMealIdeasAsync([FromBody] MealIdeasRequest request)
        {
            var (ingredients, cuisines) = await chatService.ExtractIngredientsAndCuisines(request.Prompt);
            var keyIngredients = await keyIngredientService.Upsert(ingredients);

            await cuisineService.Upsert(cuisines);

            var ideas = await chatService.GenerateIdeas(request.Prompt, ingredients, cuisines);

            var mealIdeas = await mealIdeaService.CreateRangeAsync(ideas, keyIngredients.Select(k => k.Id));

            return Ok(mealIdeas.Select(mealIdeaMapper.ToDto).ToList());
        }

        /// <summary>
        /// LLM call to preview a Recipe based on a Meal Idea. The preview is not persisted;
        /// save it with POST /api/Recipes when the user accepts it.
        /// </summary>
        [HttpPost("recipes/{mealIdeaId}")]
        public async Task<ActionResult<RecipeDTO>> GenerateRecipeAsync(int mealIdeaId)
        {
            var mealIdea = await mealIdeaService.GetByIdWithIngredientsAsync(mealIdeaId);
            if (mealIdea == null)
            {
                return NotFound();
            }

            var recipe = await chatService.GenerateRecipeAsync(mealIdea);
            if (recipe == null)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new[] { "The recipe model is unavailable or returned an invalid response." });
            }

            recipe.MealIdeaId = mealIdea.Id;

            return Ok(recipeMapper.ToDto(recipe));
        }
    }
}

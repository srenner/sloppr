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
                                       IRecipeService recipeService,
                                       MealIdeaMapper mealIdeaMapper,
                                       RecipeMapper recipeMapper) : ControllerBase
    {
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

            await recipeService.AddAsync(recipe);

            return CreatedAtAction(nameof(RecipesController.GetRecipe), "Recipes",
                new { id = recipe.Id }, recipeMapper.ToDto(recipe));
        }
    }
}

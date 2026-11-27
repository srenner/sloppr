using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Mappers;
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
                                       RecipeMapper recipeMapper,
                                       AiCapabilitySettingService capabilitySettings) : ControllerBase
    {
        private const int MaxImageBytes = 10 * 1024 * 1024;

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
        [HttpPost("recipes/{mealIdeaId:int}")]
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

        /// <summary>
        /// Reads a recipe from an image with the OCR model, then builds a recipe with
        /// the recipe generation model. The preview is not persisted; save it with
        /// POST /api/Recipes using sourceType Import.
        /// </summary>
        [HttpPost("recipes/from-image")]
        public async Task<ActionResult<RecipeDTO>> ImportRecipeFromImageAsync(
            [FromBody] RecipeImageImportRequest? request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                return BadRequest("An image is required.");
            }

            var contentType = string.IsNullOrWhiteSpace(request.ContentType)
                ? "image/png"
                : request.ContentType.Trim();

            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only image files are supported.");
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImageBase64);
            }
            catch (FormatException)
            {
                return BadRequest("The image data is not valid base64.");
            }

            if (imageBytes.Length == 0)
            {
                return BadRequest("The image is empty.");
            }

            if (imageBytes.Length > MaxImageBytes)
            {
                return BadRequest("The image must be 10 MB or smaller.");
            }

            if (capabilitySettings.Get(AiCapabilityType.OcrScanning).ModelId is null)
            {
                return BadRequest(
                    "No Recipe Image Scanner model is configured. Choose one in Settings.");
            }

            if (capabilitySettings.Get(AiCapabilityType.RecipeGeneration).ModelId is null)
            {
                return BadRequest(
                    "No Recipe Generation model is configured. Choose one in Settings.");
            }

            var recipe = await chatService.GenerateRecipeFromImageAsync(imageBytes, contentType);
            if (recipe == null)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new[] { "The recipe could not be read from that image. Try a clearer photo." });
            }

            return Ok(recipeMapper.ToDto(recipe));
        }
    }
}

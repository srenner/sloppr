using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using sloppr.AI;
using sloppr.AI.DTOs;
using sloppr.DTOs;
using sloppr.Services;
using sloppr.Settings;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiInferenceController(ChatService chatService,
                                       IAiModelService modelService,
                                       IChatClientFactory factory,
                                       IKeyIngredientService keyIngredientService,
                                       ICuisineService cuisineService,
                                       IMealIdeaService mealIdeaService,
                                       IRecipeService recipeService,
                                       MealIdeaMapper mealIdeaMapper,
                                       RecipeMapper recipeMapper,
                                       IOptions<AISettings> aiSettings) : ControllerBase
    {
        [HttpPost("meal-ideas")]
        public async Task<ActionResult<IEnumerable<MealIdeaDTO>>> GenerateMealIdeasAsync([FromBody] MealIdeasRequest request)
        {
            var ingredients = await chatService.ExtractIngredients(request.Prompt);
            var keyIngredients = await keyIngredientService.Upsert(ingredients);

            var cuisines = await chatService.ExtractCuisines(request.Prompt);
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

        [HttpPost("ingredient-extraction-challenges/{modelId}")]
        public async Task<ActionResult<IngredientExtractionResponse>> ExecuteIngredientExtractionChallengesAsync(int modelId)
        {
            var model = await modelService.GetByIdWithProviderAsync(modelId);

            if (model is null || model.AiProvider is null) return NotFound();

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
                try
                {
                    List<ChatMessage> messages = new()
                    {
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, challenge.Prompt),
                    };
                    ChatResponse response = await client.GetResponseAsync(
                        messages, new ChatOptions
                        {
                            ResponseFormat = ChatResponseFormat.ForJsonSchema<ExtractedIngredientsResponse>(
                                schemaName: "ingredients",
                                schemaDescription: "Ingredients extracted from the user prompt")
                        });
                    dto.Challenges.Add(new IngredientChallenge(challenge.Prompt, challenge.ExpectedResponse,
                        response.Text, response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount));
                }
                catch (Exception ex)
                {
                    dto.Challenges.Add(new IngredientChallenge(challenge.Prompt, challenge.ExpectedResponse,
                        string.Empty, null, null)
                    { Error = ex.Message });
                }
            }
            return Ok(dto);
        }
    }
}

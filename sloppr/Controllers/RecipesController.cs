using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController(IRecipeService svc,
                                   RecipeMapper mapper,
                                   IRecipeTemplateService templateSvc) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDTO>>> GetRecipesAsync()
        {
            var recipes = await svc.GetAllAsync();
            return Ok(recipes.Select(mapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDTO>> GetRecipe(int id)
        {
            Recipe? recipe = await svc.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return mapper.ToDto(recipe);
        }

        [HttpPost]
        public async Task<ActionResult<RecipeDTO>> PostRecipe(RecipeRequest request)
        {
            var recipe = mapper.ToEntity(request);
            await svc.AddAsync(recipe);
            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, mapper.ToDto(recipe));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, RecipeRequest request)
        {
            Recipe? recipe = await svc.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            ApplyRequest(request, recipe);

            await svc.UpdateAsync(recipe);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            Recipe? recipe = await svc.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            await svc.DeleteAsync(recipe);
            return NoContent();
        }

        [HttpGet("{id}/markdown")]
        public async Task<ActionResult<string>> GetRecipeMarkdown(int id, [FromQuery] string? template = null)
        {
            Recipe? recipe = await svc.GetByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            try
            {
                var recipeDto = mapper.ToDto(recipe);
                var markdown = await templateSvc.RenderAsync(recipeDto, template);
                return Content(markdown, "text/markdown; charset=utf-8");
            }
            catch (FileNotFoundException)
            {
                return NotFound($"Template '{template}' not found.");
            }
        }

        private void ApplyRequest(RecipeRequest request, Recipe recipe)
        {
            recipe.Name = request.Name;
            recipe.Description = request.Description;
            recipe.ServingCount = request.ServingCount;
            recipe.PrepTimeMinutes = request.PrepTimeMinutes;
            recipe.CookTimeMinutes = request.CookTimeMinutes;
            recipe.Difficulty = request.Difficulty;
            recipe.Notes = request.Notes;
            recipe.MealIdeaId = request.MealIdeaId;

            recipe.Ingredients.Clear();
            foreach (var ingredient in request.Ingredients ?? [])
            {
                recipe.Ingredients.Add(mapper.ToEntity(ingredient));
            }

            recipe.Steps.Clear();
            foreach (var step in request.Steps ?? [])
            {
                recipe.Steps.Add(mapper.ToEntity(step));
            }
        }
    }
}

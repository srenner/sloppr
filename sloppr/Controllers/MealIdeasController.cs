using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealIdeasController(IMealIdeaService svc,
                                     MealIdeaMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MealIdeaDTO>>> GetMealIdeasAsync()
        {
            var ideas = await svc.GetFilteredAsync();
            return Ok(ideas.Select(mapper.ToDto).ToList());
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<MealIdeaDTO>>> GetInactiveMealIdeasAsync()
        {
            var ideas = await svc.GetAllAsync();
            return Ok(ideas.Where(w => w.IsActive == false).Select(mapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MealIdeaDTO>> GetMealIdea(int id)
        {
            MealIdea? mealIdea = await svc.GetByIdWithIngredientsAsync(id);
            if (mealIdea == null)
            {
                return NotFound();
            }
            return mapper.ToDto(mealIdea);
        }
    }
}

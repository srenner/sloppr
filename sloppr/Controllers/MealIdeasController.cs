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

        [HttpPut("{id}")]
        public async Task<ActionResult> PutMealIdea(int id, MealIdeaUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }
            MealIdea? mealIdea = await svc.GetByIdAsync(id);
            if (mealIdea == null)
            {
                return NotFound();
            }
            mealIdea.Name = request.Name;
            await svc.UpdateAsync(mealIdea);
            return NoContent();
        }

        [HttpPut("{id}/approval")]
        public async Task<ActionResult> PutMealIdeaApproval(int id, MealIdeaApprovalRequest request)
        {
            MealIdea? mealIdea = await svc.GetByIdAsync(id);
            if (mealIdea == null)
            {
                return NotFound();
            }
            mealIdea.IsUserApproved = request.IsUserApproved;
            await svc.UpdateAsync(mealIdea);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMealIdea(int id)
        {
            MealIdea? mealIdea = await svc.GetByIdAsync(id);
            if (mealIdea == null)
            {
                return NotFound();
            }
            mealIdea.IsDeleted = true;
            await svc.UpdateAsync(mealIdea);
            return NoContent();
        }
    }
}

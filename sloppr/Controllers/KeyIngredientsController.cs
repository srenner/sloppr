using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Mappers;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyIngredientsController(IKeyIngredientService svc,
                                            KeyIngredientMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyIngredientDTO>>> GetKeyIngredients()
        {
            var ingredients = await svc.GetAllAsync();
            return Ok(ingredients.Select(mapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KeyIngredientDTO>> GetKeyIngredient(int id)
        {
            KeyIngredient? keyIngredient = await svc.GetByIdAsync(id);
            if (keyIngredient == null)
            {
                return NotFound();
            }
            return mapper.ToDto(keyIngredient);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutKeyIngredient(int id, KeyIngredient keyIngredient)
        {
            if (id != keyIngredient.Id)
            {
                return BadRequest();
            }
            await svc.UpdateAsync(keyIngredient);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<KeyIngredientDTO>> PostKeyIngredient(KeyIngredient keyIngredient)
        {
            await svc.AddAsync(keyIngredient);
            return CreatedAtAction("GetKeyIngredient", new { id = keyIngredient.Id },
                mapper.ToDto(keyIngredient));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKeyIngredient(int id)
        {
            var keyIngredient = await svc.GetByIdAsync(id);
            if (keyIngredient == null)
            {
                return NotFound();
            }
            keyIngredient.IsDeleted = true;
            await svc.UpdateAsync(keyIngredient);
            return NoContent();
        }
    }
}

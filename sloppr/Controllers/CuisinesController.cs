using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuisinesController(ICuisineService svc,
                                    CuisineMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cuisine>>> GetCuisines()
        {
            var cuisines = await svc.GetAllAsync();
            return Ok(cuisines);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CuisineDTO>> GetCuisine(int id)
        {
            Cuisine? cuisine = await svc.GetByIdAsync(id);
            if (cuisine == null)
            {
                return NotFound();
            }
            return mapper.ToDto(cuisine);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutCuisine(int id, Cuisine cuisine)
        {
            if (id != cuisine.Id)
            {
                return BadRequest();
            }
            await svc.UpdateAsync(cuisine);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Cuisine>> PostCuisine(Cuisine cuisine)
        {
            await svc.AddAsync(cuisine);
            return CreatedAtAction("GetCuisine", new { id = cuisine.Id }, cuisine);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuisine(int id)
        {
            var cuisine = await svc.GetByIdAsync(id);
            if (cuisine == null)
            {
                return NotFound();
            }
            cuisine.IsDeleted = true;
            await svc.UpdateAsync(cuisine);
            return NoContent();
        }
    }
}

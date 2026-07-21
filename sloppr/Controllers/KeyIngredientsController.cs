using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyIngredientsController(IKeyIngredientService svc) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyIngredient>>> GetKeyIngredients()
        {
            var ingredients = await svc.GetAllAsync();
            return Ok(ingredients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KeyIngredient>> GetKeyIngredient(int id)
        {
            KeyIngredient? keyIngredient = await svc.GetByIdAsync(id);
            if (keyIngredient == null)
            {
                return NotFound();
            }
            return keyIngredient;
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
        public async Task<ActionResult<KeyIngredient>> PostKeyIngredient(KeyIngredient keyIngredient)
        {
            await svc.AddAsync(keyIngredient);
            return CreatedAtAction("GetKeyIngredient", new { id = keyIngredient.Id }, keyIngredient);
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

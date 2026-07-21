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

        // PUT: api/KeyIngredients/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKeyIngredient(int id, KeyIngredient keyIngredient)
        {



            throw new NotImplementedException();
            // if (id != keyIngredient.Id)
            // {
            //     return BadRequest();
            // }

            // _context.Entry(keyIngredient).State = EntityState.Modified;

            // try
            // {
            //     await _context.SaveChangesAsync();
            // }
            // catch (DbUpdateConcurrencyException)
            // {
            //     if (!KeyIngredientExists(id))
            //     {
            //         return NotFound();
            //     }
            //     else
            //     {
            //         throw;
            //     }
            // }
            //return NoContent();
        }

        // POST: api/KeyIngredients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<KeyIngredient>> PostKeyIngredient(KeyIngredient keyIngredient)
        {
            await svc.AddAsync(keyIngredient);
            return CreatedAtAction("GetKeyIngredient", new { id = keyIngredient.Id }, keyIngredient);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKeyIngredient(int id)
        {
            throw new NotImplementedException();
            // var keyIngredient = await _context.KeyIngredients.FindAsync(id);
            // if (keyIngredient == null)
            // {
            //     return NotFound();
            // }
            // _context.KeyIngredients.Remove(keyIngredient);
            // await _context.SaveChangesAsync();

            // return NoContent();
        }

        private bool KeyIngredientExists(int id)
        {
            throw new NotImplementedException();
            //return _context.KeyIngredients.Any(e => e.Id == id);
        }
    }
}

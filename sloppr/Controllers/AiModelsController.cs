using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Mappers;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiModelsController(IAiModelService svc,
                                    AiModelMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AiModel>>> GetAiModels()
        {
            var models = await svc.GetAllAsync();
            return Ok(models);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AiModelDTO>> GetAiModel(int id)
        {
            AiModel? aiModel = await svc.GetByIdAsync(id);
            if (aiModel == null)
            {
                return NotFound();
            }
            return mapper.ToDto(aiModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAiModel(int id, AiModel aiModel)
        {
            if (id != aiModel.Id)
            {
                return BadRequest();
            }
            await svc.UpdateAsync(aiModel);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AiModel>> PostAiModel(AiModel aiModel)
        {
            await svc.AddAsync(aiModel);
            return CreatedAtAction("GetAiModel", new { id = aiModel.Id }, aiModel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAiModel(int id)
        {
            var aiModel = await svc.GetByIdAsync(id);
            if (aiModel == null)
            {
                return NotFound();
            }
            aiModel.IsDeleted = true;
            await svc.UpdateAsync(aiModel);
            return NoContent();
        }
    }
}

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
        public async Task<ActionResult<IEnumerable<AiModelDTO>>> GetActiveAiModelsAsync()
        {
            var models = await svc.GetFilteredAsync();
            return Ok(models.Select(mapper.ToDto).ToList());
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<AiModelDTO>>> GetInactiveAiModelsAsync()
        {
            var models = await svc.GetAllAsync();
            return Ok(models.Where(w => w.IsActive == false).Select(mapper.ToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AiModelDTO>> GetAiModel(int id)
        {
            AiModel? aiModel = await svc.GetByIdWithProviderAsync(id);
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
        public async Task<ActionResult<AiModelDTO>> PostAiModel(AiModel aiModel)
        {
            await svc.AddAsync(aiModel);
            var created = await svc.GetByIdWithProviderAsync(aiModel.Id);
            return CreatedAtAction("GetAiModel", new { id = aiModel.Id }, mapper.ToDto(created!));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<int>> PostAiModelBatch(ICollection<AiModelMinimalDTO> models)
        {
            var numInserted = await svc.AddRangeAsync(mapper.FromMinimalDto(models));
            return Ok(numInserted);
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

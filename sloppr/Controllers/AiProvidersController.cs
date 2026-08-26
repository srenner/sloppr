using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Mappers;
using sloppr.Models;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiProvidersController(IAiProviderService svc,
                                       AiProviderMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AiProvider>>> GetAiProviders()
        {
            var providers = await svc.GetAllAsync();
            return Ok(providers);
        }

        [HttpGet("health")]
        public async Task<ActionResult<AiProviderHealthDTO>> PrecheckProviderHealth([FromQuery] AiProviderDTO providerDto)
        {

            var provider = await svc.CheckHealthAsync(mapper.FromDto(providerDto));

            return mapper.ToHealthDto(provider);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AiProviderDTO>> GetAiProvider(int id)
        {
            AiProvider? aiProvider = await svc.GetByIdAsync(id);
            if (aiProvider == null)
            {
                return NotFound();
            }
            return mapper.ToDto(aiProvider);
        }

        [HttpGet("{id}/discover-models")]
        public async Task<ActionResult> DiscoverModels(int id)
        {
            //var provider = svc.
            throw new NotImplementedException();
        }

        [HttpPost("{id}/health")]
        public async Task<ActionResult<AiProviderHealthDTO>> GetAiProviderHealthCheck(int id)
        {
            AiProvider? aiProvider = await svc.CheckHealthAsync(id);
            if (aiProvider == null)
            {
                return NotFound();
            }
            return mapper.ToHealthDto(aiProvider);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAiProvider(int id, AiProvider aiProvider)
        {
            if (id != aiProvider.Id)
            {
                return BadRequest();
            }
            await svc.UpdateAsync(aiProvider);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AiProvider>> PostAiProvider(AiProvider aiProvider)
        {
            await svc.AddAsync(aiProvider);
            if (aiProvider.IsHealthy == true)
            {
                return CreatedAtAction(nameof(GetAiProvider), new { id = aiProvider.Id }, aiProvider);
            }
            else
            {
                var status = aiProvider.LastHealthStatusCode;
                if (status >= 400 && status < 500)
                {
                    return UnprocessableEntity(aiProvider);
                }
                else
                {
                    return StatusCode(StatusCodes.Status502BadGateway, aiProvider);
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAiProvider(int id)
        {
            var aiProvider = await svc.GetByIdAsync(id);
            if (aiProvider == null)
            {
                return NotFound();
            }
            aiProvider.IsDeleted = true;
            await svc.UpdateAsync(aiProvider);
            return NoContent();
        }
    }
}

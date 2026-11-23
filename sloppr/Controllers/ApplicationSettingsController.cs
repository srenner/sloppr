using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Mappers;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationSettingsController(ApplicationSettingService svc,
                                               ApplicationSettingMapper mapper) : ControllerBase
    {
        [HttpGet]
        public ActionResult<ApplicationSettingDTO> GetApplicationSettings()
        {
            return Ok(mapper.ToDto(svc.Settings));
        }

        [HttpPut]
        public async Task<ActionResult<ApplicationSettingDTO>> PutApplicationSettings(
            ApplicationSettingRequest request)
        {
            var settings = await svc.UpdateAsync(request);
            return Ok(mapper.ToDto(settings));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Mappers;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiCapabilitySettingsController(AiCapabilitySettingService svc,
                                                AiCapabilitySettingMapper mapper,
                                                IAiModelService modelService) : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<AiCapabilitySettingDTO>> GetAiCapabilitySettings()
        {
            return Ok(svc.GetAll().Select(mapper.ToDto).ToList());
        }

        [HttpPut]
        public async Task<ActionResult<IEnumerable<AiCapabilitySettingDTO>>> PutAiCapabilitySettings(
            List<AiCapabilitySettingRequest> requests)
        {
            var errors = await ValidateAsync(requests);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return ValidationProblem(ModelState);
            }

            var settings = await svc.UpdateAsync(requests);
            return Ok(settings.Select(mapper.ToDto).ToList());
        }

        private async Task<List<string>> ValidateAsync(List<AiCapabilitySettingRequest> requests)
        {
            var errors = new List<string>();
            var expectedCount = Enum.GetValues<AiCapabilityType>().Length;
            var distinctCount = requests.Select(request => request.Capability).Distinct().Count();

            if (requests.Count != expectedCount || distinctCount != requests.Count)
            {
                errors.Add("Provide every capability exactly once.");
            }

            foreach (var request in requests)
            {
                if (!Enum.IsDefined(request.Capability))
                {
                    errors.Add($"Unknown capability '{request.Capability}'.");
                }

                if (string.IsNullOrWhiteSpace(request.SystemPrompt))
                {
                    errors.Add($"A system prompt is required for '{request.Capability}'.");
                }
                else if (request.SystemPrompt.Length > 100_000)
                {
                    errors.Add(
                        $"The system prompt for '{request.Capability}' exceeds 100,000 characters.");
                }
            }

            var modelIds = requests
                .Where(request => request.ModelId.HasValue)
                .Select(request => request.ModelId!.Value)
                .Distinct()
                .ToList();

            if (modelIds.Count > 0)
            {
                var models = await modelService.GetFilteredAsync();
                var activeIds = models.Select(model => model.Id).ToHashSet();

                foreach (var modelId in modelIds.Where(id => !activeIds.Contains(id)))
                {
                    errors.Add($"Model {modelId} is not active.");
                }
            }

            return errors;
        }
    }
}

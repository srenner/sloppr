using Microsoft.AspNetCore.Mvc;
using sloppr.DTOs;
using sloppr.Services;
using System.Text;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeTemplatesController(IRecipeTemplateService svc,
                                          RecipeTemplateMapper mapper) : ControllerBase
    {
        private const string MarkdownContentType = "text/markdown";

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeTemplateDTO>>> GetTemplates()
        {
            var templates = await svc.GetAllAsync();
            return Ok(templates.Select(mapper.ToDto).ToList());
        }

        /// <summary>
        /// Returns the built-in starter template used to seed a new template.
        /// </summary>
        [HttpGet("starter")]
        [Produces(MarkdownContentType)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetStarter()
        {
            return Content(svc.GetStarterContent(), $"{MarkdownContentType}; charset=utf-8");
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<RecipeTemplateDTO>> GetTemplate(string name)
        {
            var template = await svc.GetByNameAsync(name);
            if (template == null)
            {
                return NotFound();
            }
            return mapper.ToDto(template);
        }

        /// <summary>
        /// Downloads a template as a .mdt file.
        /// </summary>
        [HttpGet("{name}/download")]
        [Produces(MarkdownContentType)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadTemplate(string name)
        {
            var template = await svc.GetByNameAsync(name);
            if (template == null)
            {
                return NotFound();
            }

            var bytes = Encoding.UTF8.GetBytes(template.Content);
            return File(bytes, $"{MarkdownContentType}; charset=utf-8", $"{template.Name}.mdt");
        }

        /// <summary>
        /// Imports a .mdt template file. The template name defaults to the file name.
        /// </summary>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecipeTemplateDTO>> ImportTemplate(
            [FromForm] IFormFile file,
            [FromQuery] string? name = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("A .mdt template file is required.");
            }

            var fileName = Path.GetFileName(file.FileName);
            if (!string.Equals(Path.GetExtension(fileName), ".mdt", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Template files must use the .mdt extension.");
            }

            var templateName = string.IsNullOrWhiteSpace(name)
                ? Path.GetFileNameWithoutExtension(fileName)
                : name;

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            try
            {
                var template = await svc.CreateAsync(new RecipeTemplateRequest
                {
                    Name = templateName,
                    Content = content,
                });
                return CreatedAtAction(nameof(GetTemplate), new { name = template.Name }, mapper.ToDto(template));
            }
            catch (RecipeTemplateConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (RecipeTemplateValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<RecipeTemplateDTO>> PostTemplate(RecipeTemplateRequest request)
        {
            try
            {
                var template = await svc.CreateAsync(request);
                return CreatedAtAction(nameof(GetTemplate), new { name = template.Name }, mapper.ToDto(template));
            }
            catch (RecipeTemplateConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (RecipeTemplateValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<RecipeTemplateDTO>> PutTemplate(string name, RecipeTemplateUpdateRequest request)
        {
            try
            {
                var template = await svc.UpdateAsync(name, request.Content);
                if (template == null)
                {
                    return NotFound();
                }
                return mapper.ToDto(template);
            }
            catch (RecipeTemplateValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteTemplate(string name)
        {
            try
            {
                return await svc.DeleteAsync(name) ? NoContent() : NotFound();
            }
            catch (RecipeTemplateValidationException)
            {
                return NotFound();
            }
        }
    }
}

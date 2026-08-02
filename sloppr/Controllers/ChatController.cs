using Microsoft.AspNetCore.Mvc;
using sloppr.Enums;
using sloppr.Services;

namespace sloppr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ChatService chatService, IAiModelService modelService) : ControllerBase
    {
        /// <remarks>
        /// TEMPORARY — for local testing only. Not part of core functionality. Safe to delete.
        /// </remarks>
        [HttpPost("chat")]
        public IActionResult SendChat(AiProviderType type, string prompt)
        {
            var path = chatService.GetChatPath(type);
            // build full URL, call Ollama, etc.
            return Ok();
        }

        /// <remarks>
        /// TEMPORARY — for local testing only. Not part of core functionality. Safe to delete.
        /// </remarks>
        [HttpPost("chat2")]
        public async Task<IActionResult> SendChat2(int modelId, [FromBody] string prompt)
        {
            var model = await modelService.GetByIdWithProviderAsync(modelId);
            if (model != null)
            {

            }
            return Ok();
        }
    }
}

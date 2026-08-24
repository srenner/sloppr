using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.AI;
using sloppr.Enums;

namespace sloppr.AI.DTOs;

public class ChatRequest
{
    [Required]
    public int ModelId { get; set; }

    [Required]
    [MinLength(1)]
    public List<ChatMessage> Messages { get; set; } = new();

    public bool? UseJsonSchemaResponseFormat { get; set; } = false;
}

// public class ChatMessageDTO
// {
//     [Required]
//     public ChatRole Role { get; set; } = ChatRole.User;

//     [Required]
//     public string Content { get; set; } = "";
// }

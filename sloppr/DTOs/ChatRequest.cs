using System;
using System.ComponentModel.DataAnnotations;
using sloppr.Enums;

namespace sloppr.DTOs;

public class ChatRequest
{
    [Required]
    public AiProviderType Provider { get; set; }

    [Required]
    public string Model { get; set; }

    [Required]
    [MinLength(1)]
    public List<ChatMessageDTO> Messages { get; set; } = new();

    public bool? UseJsonSchemaResponseFormat { get; set; } = false;
}

public class ChatMessageDTO
{
    [Required]
    public AiRoleType Role { get; set; } = AiRoleType.user;

    [Required]
    public string Content { get; set; } = "";
}

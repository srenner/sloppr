using System.ComponentModel.DataAnnotations;

namespace sloppr.DTOs;

public class RecipeImageImportRequest
{
    [Required]
    public required string ImageBase64 { get; set; }

    [Required]
    public required string ContentType { get; set; }
}

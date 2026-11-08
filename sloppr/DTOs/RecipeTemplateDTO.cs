namespace sloppr.DTOs;

public class RecipeTemplateDTO
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public required string Name { get; set; }
    public required string Content { get; set; }
}

namespace sloppr.DTOs;

public class RecipeDTO
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }

    public required string FullText { get; set; }
    public string TextFormat { get; set; } = "md";
    public int PrepTime { get; set; } = 0;
    public int CookTime { get; set; } = 0;
}
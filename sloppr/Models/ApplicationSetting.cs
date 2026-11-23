namespace sloppr.Models;

public class ApplicationSetting
{
    public int Id { get; set; } = 1;
    public string? DefaultRecipeTemplateName { get; set; }
    public bool HasSeededRecipeTemplates { get; set; }
}

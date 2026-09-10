using System;
using Humanizer;

namespace sloppr.Models;

public class ApplicationSetting
{
    public int Id { get; set; } = 1;
    public string ApplicationName { get; set; } = "sloppr";
    public int? ExtractionModelId { get; set; }
    public int? IdeaModelId { get; set; }
    public int? RecipeModelId { get; set; }
}

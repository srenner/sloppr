using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class RecipeTemplateMapper
{
    public partial RecipeTemplateDTO ToDto(RecipeTemplate template);
    public partial RecipeTemplate ToEntity(RecipeTemplateRequest request);
}

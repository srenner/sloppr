using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class RecipeMapper
{
    public partial RecipeDTO ToDto(Recipe recipe);
    public partial Recipe ToEntity(RecipeRequest request);
    public partial Recipe ToEntity(RecipeGenerationResponse response);
    public partial RecipeIngredient ToEntity(RecipeIngredientRequest request);
    public partial RecipeStep ToEntity(RecipeStepRequest request);
}

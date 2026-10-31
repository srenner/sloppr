using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class RecipeMapper
{
    public partial RecipeDTO ToDto(Recipe recipe);
}

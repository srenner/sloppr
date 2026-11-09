using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class KeyIngredientMapper
{
    public partial KeyIngredientDTO ToDto(KeyIngredient ingredient);
    public partial KeyIngredient ToEntity(KeyIngredientRequest request);
    public partial void UpdateEntity(KeyIngredientRequest request, KeyIngredient ingredient);
}
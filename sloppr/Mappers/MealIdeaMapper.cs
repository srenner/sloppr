using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class MealIdeaMapper
{
    public partial MealIdeaDTO ToDto(MealIdea idea);
}
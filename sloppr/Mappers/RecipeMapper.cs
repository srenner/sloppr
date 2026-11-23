using sloppr.Content;
using sloppr.DTOs;
using sloppr.Enums;
using sloppr.Models;

namespace sloppr.Mappers;

public class RecipeMapper
{
    public RecipeDTO ToDto(Recipe recipe)
    {
        return new RecipeDTO
        {
            Id = recipe.Id,
            DateCreated = recipe.DateCreated,
            DateUpdated = recipe.DateUpdated,
            IsActive = recipe.IsActive,
            IsDeleted = recipe.IsDeleted,
            CreatedBy = recipe.CreatedBy,
            UpdatedBy = recipe.UpdatedBy,
            MealIdeaId = recipe.MealIdeaId,
            TemplateName = recipe.TemplateName,
            SourceType = recipe.SourceType,
            Content = RecipeContentJson.Deserialize(recipe.ContentJson)
        };
    }

    public RecipeSummaryDTO ToSummaryDto(Recipe recipe)
    {
        var content = RecipeContentJson.Deserialize(recipe.ContentJson);

        return new RecipeSummaryDTO
        {
            Id = recipe.Id,
            Name = content.Name,
            Description = content.Description,
            SourceType = recipe.SourceType,
            ServingCount = content.ServingCount,
            PrepTimeMinutes = content.PrepTimeMinutes,
            CookTimeMinutes = content.CookTimeMinutes,
            Difficulty = content.Difficulty,
            DateUpdated = recipe.DateUpdated
        };
    }

    public Recipe ToEntity(RecipeRequest request)
    {
        return new Recipe
        {
            ContentJson = RecipeContentJson.Serialize(request.Content),
            SourceType = request.MealIdeaId.HasValue ? RecipeSourceType.Llm : RecipeSourceType.Manual,
            MealIdeaId = request.MealIdeaId,
            TemplateName = request.TemplateName
        };
    }

    public Recipe ToEntity(RecipeContent content)
    {
        return new Recipe
        {
            ContentJson = RecipeContentJson.Serialize(content),
            SourceType = RecipeSourceType.Llm
        };
    }
}

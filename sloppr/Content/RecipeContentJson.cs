using System.Text.Json;

namespace sloppr.Content;

public static class RecipeContentJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static RecipeContent Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new RecipeContent();
        }

        var content = JsonSerializer.Deserialize<RecipeContent>(json, Options) ?? new RecipeContent();
        content.Ingredients ??= [];
        content.Steps ??= [];
        return content;
    }

    public static string Serialize(RecipeContent content)
    {
        Normalize(content);
        return JsonSerializer.Serialize(content, Options);
    }

    public static void Normalize(RecipeContent content)
    {
        content.Name = content.Name?.Trim() ?? string.Empty;
        content.Description = content.Description?.Trim() ?? string.Empty;
        content.Notes = TrimToNull(content.Notes);
        content.Ingredients ??= [];
        content.Steps ??= [];

        foreach (var ingredient in content.Ingredients)
        {
            ingredient.Name = ingredient.Name?.Trim() ?? string.Empty;
            ingredient.Unit = TrimToNull(ingredient.Unit);
            ingredient.Group = TrimToNull(ingredient.Group);
            ingredient.Note = TrimToNull(ingredient.Note);
        }

        foreach (var step in content.Steps)
        {
            step.Instruction = step.Instruction?.Trim() ?? string.Empty;
            step.Group = TrimToNull(step.Group);
        }
    }

    public static List<string> Validate(RecipeContent content)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content.Name))
        {
            errors.Add("Recipe name is required.");
        }

        if (string.IsNullOrWhiteSpace(content.Description))
        {
            errors.Add("Recipe description is required.");
        }

        if (content.ServingCount < 1 || content.ServingCount > 100)
        {
            errors.Add("Serving count must be between 1 and 100.");
        }

        if (content.PrepTimeMinutes < 0 || content.PrepTimeMinutes > 1440)
        {
            errors.Add("Prep time must be between 0 and 1440 minutes.");
        }

        if (content.CookTimeMinutes < 0 || content.CookTimeMinutes > 1440)
        {
            errors.Add("Cook time must be between 0 and 1440 minutes.");
        }

        if (content.Difficulty.HasValue && (content.Difficulty.Value < 1 || content.Difficulty.Value > 3))
        {
            errors.Add("Difficulty must be between 1 and 3.");
        }

        foreach (var ingredient in content.Ingredients ?? [])
        {
            if (string.IsNullOrWhiteSpace(ingredient.Name))
            {
                errors.Add("Ingredient name is required.");
            }
        }

        foreach (var step in content.Steps ?? [])
        {
            if (string.IsNullOrWhiteSpace(step.Instruction))
            {
                errors.Add("Step instruction is required.");
            }
        }

        return errors;
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

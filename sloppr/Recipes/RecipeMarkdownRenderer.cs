using System.Globalization;
using System.Text;
using sloppr.Models;

namespace sloppr.Recipes;

public static class RecipeMarkdownRenderer
{
    public static string Render(Recipe recipe)
    {
        var markdown = new StringBuilder();

        markdown.AppendLine($"# {recipe.Name}");
        markdown.AppendLine();
        markdown.AppendLine(recipe.Description);
        markdown.AppendLine();
        markdown.AppendLine($"**Servings:** {recipe.ServingCount} | **Prep:** {recipe.PrepTimeMinutes} min "
            + $"| **Cook:** {recipe.CookTimeMinutes} min | **Difficulty:** {recipe.Difficulty}/3");

        if (!string.IsNullOrWhiteSpace(recipe.Notes))
        {
            markdown.AppendLine();
            markdown.AppendLine($"**Notes:** {recipe.Notes}");
        }

        markdown.AppendLine();
        markdown.AppendLine("## Ingredients");
        markdown.AppendLine();

        string? currentGroup = null;
        var isFirst = true;
        foreach (var ingredient in recipe.Ingredients.OrderBy(i => i.Order))
        {
            AppendGroupHeading(markdown, ingredient.Group, ref currentGroup, isFirst);
            markdown.AppendLine($"- {FormatIngredient(ingredient)}");
            isFirst = false;
        }

        markdown.AppendLine();
        markdown.AppendLine("## Steps");
        markdown.AppendLine();

        currentGroup = null;
        isFirst = true;
        var stepNumber = 0;
        foreach (var step in recipe.Steps.OrderBy(s => s.Order))
        {
            AppendGroupHeading(markdown, step.Group, ref currentGroup, isFirst);
            stepNumber++;
            markdown.AppendLine($"{stepNumber}. {step.Instruction}");
            isFirst = false;
        }

        return markdown.ToString().TrimEnd() + "\n";
    }

    private static void AppendGroupHeading(StringBuilder markdown, string? group,
                                           ref string? currentGroup, bool isFirst)
    {
        if (string.Equals(currentGroup, group, StringComparison.Ordinal))
        {
            return;
        }

        currentGroup = group;

        if (string.IsNullOrWhiteSpace(group))
        {
            return;
        }

        if (!isFirst)
        {
            markdown.AppendLine();
        }

        markdown.AppendLine($"### {group}");
        markdown.AppendLine();
    }

    private static string FormatIngredient(RecipeIngredient ingredient)
    {
        var parts = new List<string>(4);

        if (ingredient.Quantity is double quantity)
        {
            parts.Add(quantity.ToString("0.##", CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrWhiteSpace(ingredient.Unit))
        {
            parts.Add(ingredient.Unit);
        }

        parts.Add(ingredient.Name);

        var text = string.Join(" ", parts);

        if (!string.IsNullOrWhiteSpace(ingredient.Note))
        {
            text += $" ({ingredient.Note})";
        }

        return text;
    }
}

namespace sloppr.Services;

using System.Text.RegularExpressions;
using sloppr.DTOs;

public interface IRecipeTemplateService
{
    Task<string> RenderAsync(RecipeDTO recipe, string? templateName = null);
}

public class RecipeTemplateService : IRecipeTemplateService
{
    private readonly string _templatesFolder;

    public RecipeTemplateService(IWebHostEnvironment env)
    {
        _templatesFolder = Path.Combine(env.ContentRootPath, "Templates", "Recipes");
    }

    public async Task<string> RenderAsync(RecipeDTO recipe, string? templateName = null)
    {
        templateName = string.IsNullOrWhiteSpace(templateName) ? "default" : templateName;
        // Sanitize templateName to prevent path traversal
        templateName = Path.GetFileNameWithoutExtension(templateName);

        var templatePath = Path.Combine(_templatesFolder, $"{templateName}.md");
        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(_templatesFolder, "default.md");
        }

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Recipe template '{templateName}' not found.");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        return RenderTemplate(templateContent, recipe);
    }

    private static string RenderTemplate(string template, RecipeDTO recipe)
    {
        // Simple mustache-like parsing
        // 1. Process sections: {{#Section}}...{{/Section}}
        template = ProcessSections(template, recipe);

        // 2. Process scalar variables: {{Variable}}
        template = ProcessVariables(template, recipe);

        // Clean up extra blank lines resulting from empty sections
        template = Regex.Replace(template, @"\n{3,}", "\n\n");

        return template.Trim();
    }

    private static string ProcessSections(string template, RecipeDTO recipe)
    {
        // Difficulty section
        template = ProcessBooleanOrNullableSection(template, "Difficulty", recipe.Difficulty.HasValue, () =>
        {
            return RenderVariablesForObject(GetSectionContent(template, "Difficulty"), recipe);
        });

        // Notes section
        template = ProcessBooleanOrNullableSection(template, "Notes", !string.IsNullOrWhiteSpace(recipe.Notes), () =>
        {
            var content = GetSectionContent(template, "Notes");
            return content.Replace("{{Notes}}", recipe.Notes ?? string.Empty);
        });

        // Ingredients section (list)
        template = ProcessListSection(template, "Ingredients", recipe.Ingredients, (itemContent, item) =>
        {
            var rendered = itemContent;
            rendered = ProcessBooleanOrNullableSection(rendered, "Quantity", item.Quantity.HasValue, () =>
                GetSectionContent(rendered, "Quantity").Replace("{{Quantity}}", item.Quantity?.ToString() ?? ""));
            rendered = ProcessBooleanOrNullableSection(rendered, "Unit", !string.IsNullOrWhiteSpace(item.Unit), () =>
                GetSectionContent(rendered, "Unit").Replace("{{Unit}}", item.Unit ?? ""));
            rendered = ProcessBooleanOrNullableSection(rendered, "Group", !string.IsNullOrWhiteSpace(item.Group), () =>
                GetSectionContent(rendered, "Group").Replace("{{Group}}", item.Group ?? ""));
            rendered = ProcessBooleanOrNullableSection(rendered, "Note", !string.IsNullOrWhiteSpace(item.Note), () =>
                GetSectionContent(rendered, "Note").Replace("{{Note}}", item.Note ?? ""));

            rendered = rendered.Replace("{{Name}}", item.Name ?? "");
            return rendered.TrimEnd('\r', '\n');
        });

        // Steps section (list)
        template = ProcessListSection(template, "Steps", recipe.Steps, (itemContent, item) =>
        {
            var rendered = itemContent;
            rendered = ProcessBooleanOrNullableSection(rendered, "Group", !string.IsNullOrWhiteSpace(item.Group), () =>
                GetSectionContent(rendered, "Group").Replace("{{Group}}", item.Group ?? ""));

            rendered = rendered.Replace("{{Instruction}}", item.Instruction ?? "");
            return rendered.TrimEnd('\r', '\n');
        });

        return template;
    }

    private static string ProcessBooleanOrNullableSection(string template, string tagName, bool condition, Func<string> renderInner)
    {
        var pattern = $@"{{#({tagName})}}(.*?){{/\1}}";
        return Regex.Replace(template, pattern, match =>
        {
            if (!condition) return string.Empty;
            return renderInner();
        }, RegexOptions.Singleline);
    }

    private static string ProcessListSection<T>(string template, string tagName, IEnumerable<T> items, Func<string, T, string> renderItem)
    {
        var pattern = $@"{{#({tagName})}}(.*?){{/\1}}";
        return Regex.Replace(template, pattern, match =>
        {
            var list = items?.ToList();
            if (list == null || list.Count == 0) return string.Empty;

            var innerTemplate = match.Groups[2].Value;
            var results = new List<string>();
            foreach (var item in list)
            {
                results.Add(renderItem(innerTemplate, item));
            }
            return string.Join(Environment.NewLine, results);
        }, RegexOptions.Singleline);
    }

    private static string GetSectionContent(string template, string tagName)
    {
        var match = Regex.Match(template, $@"{{#{tagName}}}(.*?){{/{tagName}}}", RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string RenderVariablesForObject(string template, object obj)
    {
        // Simple property replacement
        var props = obj.GetType().GetProperties();
        foreach (var prop in props)
        {
            var val = prop.GetValue(obj)?.ToString() ?? string.Empty;
            template = template.Replace($"{{{{{prop.Name}}}}}", val);
        }
        return template;
    }

    private static string ProcessVariables(string template, RecipeDTO recipe)
    {
        template = template.Replace("{{Name}}", recipe.Name ?? "");
        template = template.Replace("{{Description}}", recipe.Description ?? "");
        template = template.Replace("{{Servings}}", recipe.ServingCount.ToString());
        template = template.Replace("{{PrepMinutes}}", recipe.PrepTimeMinutes.ToString());
        template = template.Replace("{{CookMinutes}}", recipe.CookTimeMinutes.ToString());
        template = template.Replace("{{Difficulty}}", recipe.Difficulty?.ToString() ?? "");
        template = template.Replace("{{Notes}}", recipe.Notes ?? "");
        return template;
    }
}

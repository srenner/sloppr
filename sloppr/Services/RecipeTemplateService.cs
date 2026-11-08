namespace sloppr.Services;

using Fluid;
using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Models;

public class RecipeTemplateService : IRecipeTemplateService
{
    private const string DefaultTemplateName = "default";
    private const string StarterResourceSuffix = ".Recipes.default.mdt";
    private const int MaxTemplateLength = 100_000;

    private static readonly Lazy<string> StarterContent = new(LoadStarterContent);
    private static readonly FluidParser Parser = new();

    // Templates are user-authored, so apply the execution limits recommended for
    // untrusted input. The options object is immutable and safe to share.
    private static readonly TemplateOptions TemplateOptions = new TemplateOptionsBuilder()
        .WithMaxRecursion(20)
        .WithMaxSteps(10_000)
        .WithMaxOutputSize(1_000_000)
        .WithMaxCollectionSize(10_000)
        .Build();

    private readonly ApplicationDbContext _db;

    public RecipeTemplateService(ApplicationDbContext db) => _db = db;

    public async Task<string> RenderAsync(RecipeDTO recipe, string? templateName = null)
    {
        RecipeTemplate? template;

        if (!string.IsNullOrWhiteSpace(templateName))
        {
            template = await GetByNameAsync(templateName.Trim());
            if (template == null)
            {
                throw new RecipeTemplateNotFoundException(
                    $"Recipe template '{templateName}' was not found.");
            }
        }
        else
        {
            template = await ResolveAsync(recipe);
            if (template == null)
            {
                throw new RecipeTemplateNotFoundException(
                    "No recipe template is configured. Create or import one first.");
            }
        }

        return Render(template.Content, recipe);
    }

    public async Task<IReadOnlyList<RecipeTemplate>> GetAllAsync()
    {
        return await _db.RecipeTemplates
            .AsNoTracking()
            .OrderByDescending(t => t.Name == DefaultTemplateName)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<RecipeTemplate?> GetByNameAsync(string name)
    {
        return await _db.RecipeTemplates.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<RecipeTemplate> CreateAsync(RecipeTemplateRequest request)
    {
        var name = NormalizeName(request.Name);
        ValidateTemplate(request.Content);

        if (await _db.RecipeTemplates.AnyAsync(t => t.Name == name))
        {
            throw new RecipeTemplateConflictException(
                $"A recipe template named '{name}' already exists.");
        }

        var template = new RecipeTemplate { Name = name, Content = request.Content };
        _db.RecipeTemplates.Add(template);
        await _db.SaveChangesAsync();
        return template;
    }

    public async Task<RecipeTemplate?> UpdateAsync(string name, string content)
    {
        ValidateTemplate(content);

        var template = await GetByNameAsync(NormalizeName(name));
        if (template == null)
        {
            return null;
        }

        template.Content = content;
        await _db.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteAsync(string name)
    {
        var template = await GetByNameAsync(NormalizeName(name));
        if (template == null)
        {
            return false;
        }

        _db.RecipeTemplates.Remove(template);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Inserts the seeded starter template once per database. Deliberately does not
    /// run when templates already exist or after the one-time seed has happened, so a
    /// user who deletes every template does not get it resurrected on restart.
    /// </summary>
    public async Task SeedDefaultAsync()
    {
        var settings = await _db.ApplicationSettings.SingleAsync();
        if (settings.HasSeededRecipeTemplates)
        {
            return;
        }

        if (!await _db.RecipeTemplates.AnyAsync(t => t.Name == DefaultTemplateName))
        {
            _db.RecipeTemplates.Add(new RecipeTemplate
            {
                Name = DefaultTemplateName,
                Content = StarterContent.Value,
            });
        }

        if (string.IsNullOrWhiteSpace(settings.DefaultRecipeTemplateName))
        {
            settings.DefaultRecipeTemplateName = DefaultTemplateName;
        }

        settings.HasSeededRecipeTemplates = true;
        await _db.SaveChangesAsync();
    }

    public string GetStarterContent() => StarterContent.Value;

    private async Task<RecipeTemplate?> ResolveAsync(RecipeDTO recipe)
    {
        if (!string.IsNullOrWhiteSpace(recipe.TemplateName))
        {
            var overrideTemplate = await GetByNameAsync(recipe.TemplateName);
            if (overrideTemplate != null)
            {
                return overrideTemplate;
            }
        }

        var settings = await _db.ApplicationSettings.SingleAsync();
        if (!string.IsNullOrWhiteSpace(settings.DefaultRecipeTemplateName))
        {
            var defaultTemplate = await GetByNameAsync(settings.DefaultRecipeTemplateName);
            if (defaultTemplate != null)
            {
                return defaultTemplate;
            }
        }

        return await GetByNameAsync(DefaultTemplateName);
    }

    private static string Render(string template, RecipeDTO recipe)
    {
        var parsed = ParseOrThrow(template);
        var model = BuildModel(recipe);

        return parsed.Render(new TemplateContext(model, TemplateOptions)).Trim();
    }

    private static IFluidTemplate ParseOrThrow(string template)
    {
        if (!Parser.TryParse(template, out var parsed, out var error))
        {
            throw new RecipeTemplateValidationException(
                $"Template is not valid Liquid: {error}");
        }

        return parsed;
    }

    private static object BuildModel(RecipeDTO recipe)
    {
        return new
        {
            name = recipe.Name,
            description = recipe.Description,
            serving_count = recipe.ServingCount,
            prep_time_minutes = recipe.PrepTimeMinutes,
            cook_time_minutes = recipe.CookTimeMinutes,
            difficulty = recipe.Difficulty,
            notes = recipe.Notes,
            ingredients = (recipe.Ingredients ?? []).Select(i => new
            {
                quantity = i.Quantity,
                unit = i.Unit,
                name = i.Name,
                group = i.Group,
                note = i.Note,
            }),
            steps = (recipe.Steps ?? []).Select(s => new
            {
                instruction = s.Instruction,
                group = s.Group,
            }),
        };
    }

    private static string NormalizeName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();

        if (trimmed.Length == 0)
        {
            throw new RecipeTemplateValidationException("Template name is required.");
        }

        if (trimmed.Length > 100)
        {
            throw new RecipeTemplateValidationException(
                "Template name must be 100 characters or fewer.");
        }

        if (trimmed.Any(c => !(char.IsLetterOrDigit(c) || c is ' ' or '-' or '_')))
        {
            throw new RecipeTemplateValidationException(
                "Template name may only contain letters, digits, spaces, hyphens, and underscores.");
        }

        return trimmed;
    }

    private static void ValidateTemplate(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new RecipeTemplateValidationException("Template content is required.");
        }

        if (content.Length > MaxTemplateLength)
        {
            throw new RecipeTemplateValidationException(
                $"Template content must be {MaxTemplateLength} characters or fewer.");
        }

        ParseOrThrow(content);
    }

    private static string LoadStarterContent()
    {
        var assembly = typeof(RecipeTemplateService).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(StarterResourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InvalidOperationException(
                "The seeded recipe template resource could not be found.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                "The seeded recipe template resource could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

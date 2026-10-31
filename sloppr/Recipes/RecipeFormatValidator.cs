using sloppr.Models;

namespace sloppr.Recipes;

public static class RecipeFormatValidator
{
    public static IReadOnlyList<string> Validate(Recipe recipe)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(recipe.Name) || recipe.Name.Length > RecipeFormatRules.MaxNameLength)
        {
            errors.Add($"Name is required and must be {RecipeFormatRules.MaxNameLength} characters or fewer.");
        }

        if (string.IsNullOrWhiteSpace(recipe.Description)
            || recipe.Description.Length > RecipeFormatRules.MaxDescriptionLength)
        {
            errors.Add($"Description is required and must be {RecipeFormatRules.MaxDescriptionLength} characters or fewer.");
        }

        if (recipe.ServingCount < 1) errors.Add("Serving count must be 1 or more.");
        if (recipe.PrepTimeMinutes < 0) errors.Add("Prep time cannot be negative.");
        if (recipe.CookTimeMinutes < 0) errors.Add("Cook time cannot be negative.");
        if (recipe.Difficulty is < 1 or > 3) errors.Add("Difficulty must be between 1 and 3.");

        if (recipe.Notes?.Length > RecipeFormatRules.MaxNotesLength)
        {
            errors.Add($"Notes must be {RecipeFormatRules.MaxNotesLength} characters or fewer.");
        }

        ValidateIngredients(recipe.Ingredients, errors);
        ValidateSteps(recipe.Steps, errors);

        return errors;
    }

    private static void ValidateIngredients(ICollection<RecipeIngredient> ingredients, List<string> errors)
    {
        if (ingredients.Count == 0)
        {
            errors.Add("At least one ingredient is required.");
            return;
        }

        var orders = new HashSet<int>();
        var position = 0;
        foreach (var ingredient in ingredients)
        {
            position++;
            var label = $"Ingredient {position}";

            if (ingredient.Order < 1 || !orders.Add(ingredient.Order))
            {
                errors.Add($"{label} order must be 1 or greater and unique.");
            }

            if (string.IsNullOrWhiteSpace(ingredient.Name)
                || ingredient.Name.Length > RecipeFormatRules.MaxIngredientNameLength)
            {
                errors.Add($"{label} name is required and must be {RecipeFormatRules.MaxIngredientNameLength} characters or fewer.");
            }

            if (ingredient.Quantity is <= 0) errors.Add($"{label} quantity must be greater than 0.");

            if (ingredient.Unit?.Length > RecipeFormatRules.MaxUnitLength)
            {
                errors.Add($"{label} unit must be {RecipeFormatRules.MaxUnitLength} characters or fewer.");
            }

            if (ingredient.Group?.Length > RecipeFormatRules.MaxGroupLength)
            {
                errors.Add($"{label} group must be {RecipeFormatRules.MaxGroupLength} characters or fewer.");
            }

            if (ingredient.Note?.Length > RecipeFormatRules.MaxIngredientNoteLength)
            {
                errors.Add($"{label} note must be {RecipeFormatRules.MaxIngredientNoteLength} characters or fewer.");
            }
        }
    }

    private static void ValidateSteps(ICollection<RecipeStep> steps, List<string> errors)
    {
        if (steps.Count == 0)
        {
            errors.Add("At least one step is required.");
            return;
        }

        var orders = new HashSet<int>();
        var position = 0;
        foreach (var step in steps)
        {
            position++;
            var label = $"Step {position}";

            if (step.Order < 1 || !orders.Add(step.Order))
            {
                errors.Add($"{label} order must be 1 or greater and unique.");
            }

            if (string.IsNullOrWhiteSpace(step.Instruction)
                || step.Instruction.Length > RecipeFormatRules.MaxInstructionLength)
            {
                errors.Add($"{label} instruction is required and must be {RecipeFormatRules.MaxInstructionLength} characters or fewer.");
            }

            if (step.Group?.Length > RecipeFormatRules.MaxGroupLength)
            {
                errors.Add($"{label} group must be {RecipeFormatRules.MaxGroupLength} characters or fewer.");
            }
        }
    }
}

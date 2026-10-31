namespace sloppr.Recipes;

/// <summary>
/// Limits enforced by <see cref="RecipeFormatValidator"/>. The stored rules JSON is a
/// historical snapshot; these values are the ones the app enforces.
/// </summary>
public static class RecipeFormatRules
{
    public const int MaxNameLength = 120;
    public const int MaxDescriptionLength = 500;
    public const int MaxNotesLength = 1000;
    public const int MaxGroupLength = 50;
    public const int MaxIngredientNameLength = 120;
    public const int MaxUnitLength = 40;
    public const int MaxIngredientNoteLength = 200;
    public const int MaxInstructionLength = 1000;
}

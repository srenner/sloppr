namespace sloppr.Models;

/// <summary>
/// Join row linking a <see cref="MealIdea"/> to a shared <see cref="KeyIngredient"/>.
/// </summary>
public class MealIdeaKeyIngredient
{
    public int MealIdeaId { get; set; }
    public int KeyIngredientId { get; set; }
}

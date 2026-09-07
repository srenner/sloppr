using System;

namespace sloppr.Models;

public class MealIdea : BaseModel
{
    public string Name { get; set; }

    /// <summary>Records 👍 / 👎 from user</summary>
    public bool? IsUserApproved { get; set; }

    public ICollection<KeyIngredient> KeyIngredients { get; set; }

}

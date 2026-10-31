namespace sloppr.Recipes;

/// <summary>
/// Names the current recipe format and seeds its rules snapshot.
/// </summary>
public static class RecipeFormat
{
    public const string Key = "standard";
    public const int Version = 1;

    public static readonly DateTime SeedDate = new(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Historical snapshot stored with version 1. Not read by the app; validation uses
    /// <see cref="RecipeFormatRules"/>.
    /// </summary>
    public const string StandardV1RulesJson = """
        {
          "info": {
            "name": { "required": true, "maxLength": 120 },
            "description": { "required": true, "maxLength": 500 },
            "servingCount": { "min": 1 },
            "prepTimeMinutes": { "min": 0 },
            "cookTimeMinutes": { "min": 0 },
            "difficulty": { "min": 1, "max": 3 },
            "notes": { "maxLength": 1000 }
          },
          "ingredients": {
            "minCount": 1,
            "fields": {
              "order": { "min": 1, "unique": true },
              "quantity": { "minExclusive": 0 },
              "unit": { "maxLength": 40 },
              "name": { "required": true, "maxLength": 120 },
              "group": { "maxLength": 50 },
              "note": { "maxLength": 200 }
            }
          },
          "steps": {
            "minCount": 1,
            "fields": {
              "order": { "min": 1, "unique": true },
              "instruction": { "required": true, "maxLength": 1000 },
              "group": { "maxLength": 50 }
            }
          }
        }
        """;
}

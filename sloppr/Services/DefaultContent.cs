namespace sloppr.Services;

/// <summary>
/// DB Seed data
/// </summary>
public static class DefaultContent
{
    public const string RecipeTemplate = """
        # {{ name }}

        {{ description }}

        - **Servings:** {{ serving_count }}
        - **Prep Time:** {{ prep_time_minutes }} mins
        - **Cook Time:** {{ cook_time_minutes }} mins
        {% if difficulty %}- **Difficulty:** {{ difficulty }}/3{% endif %}

        ## Ingredients

        {% for ingredient in ingredients %}
        - {% if ingredient.quantity %}{{ ingredient.quantity }} {% endif %}{% if ingredient.unit %}{{ ingredient.unit }} {% endif %}{{ ingredient.name }}{% if ingredient.group %} *({{ ingredient.group }})*{% endif %}{% if ingredient.note %} — {{ ingredient.note }}{% endif %}
        {% endfor %}

        ## Steps

        {% for step in steps %}
        1. {% if step.group %}**{{ step.group }}:** {% endif %}{{ step.instruction }}
        {% endfor %}

        {% if notes %}## Notes

        {{ notes }}
        {% endif %}
        """;

    public const string IngredientExtractionPrompt = """Extract all grocery/food ingredients and cuisine types mentioned in the user prompt. Return a JSON object with two properties: "ingredients" and "cuisines", each an array of strings, for example {"ingredients":["red onions","peppers"],"cuisines":["mexican"]}. For ingredients, use one entry per distinct ingredient in the order mentioned and keep the user's own wording, dropping quantities, units, and prep. Use an empty array for cuisines when none are mentioned. Return nothing else.""";

    public const string IdeaGenerationPrompt = """You are a family meal planner. Given a list of ingredients, identify 3 recognizable dishes or meal types that can be made from them. The dishes should incorporate as many of the key ingredients as possible, with remaining ingredients being common household staples. The dish name and recipe should be similar or identical to recipes that are common in cookbooks. Return a JSON object with a single property named "ideas" whose value is an array of recipe name strings, for example {"ideas":["Beef & Carrot Fried Rice","Korean-Style Beef Rice Bowls"]}. Return nothing else.""";

    public const string RecipeGenerationPrompt = """You are a chef. Given a meal idea and key ingredients, generate one complete recipe as a single strict JSON object with these top-level properties: name, description, servingCount, prepTimeMinutes, cookTimeMinutes, optional notes, ingredients (ordered array of objects with order, optional quantity, optional unit, name, optional group, optional note), and steps (ordered array of objects with order, instruction, optional group). Output the info fields at the top level, not under an info wrapper. Leave group blank for the main recipe and use a short label such as Sauce or Marinade only when the dish has parts. Return ONLY JSON with no prose, commentary, markdown code fences, or intro text.""";

    public const string OcrScanningPrompt = """You are an OCR assistant. Extract the recipe from the image, including the recipe name, ingredients with quantities and units, and ordered instructions. Preserve the original wording where possible and return plain text only.""";
}

namespace sloppr.Services;

/// <summary>
/// Default prompts and DB seed data.
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

    public const string RecipeGenerationPrompt = """
        You are a chef. Given a meal idea, key ingredients, or existing recipe text, generate one complete recipe as a single strict JSON object with these top-level properties: name, description, servingCount, prepTimeMinutes, cookTimeMinutes, optional notes, ingredients, and steps.

        Each ingredient object must have a non-empty name. Put the food item itself in name (for example "brussel sprouts", "bacon", "apple", "olive oil"); put amounts in quantity and unit; use note only for preparation, substitutions, or other details. Never put the whole ingredient line in note and never leave name empty. Parse each ingredient line into exactly one ingredient object; never split one line across multiple ingredients. Order ingredients as they are used.

        Each step object must have a non-empty instruction and an optional group. Order steps in cooking order.

        Write description as one or two specific sentences (about 15-40 words) that mention the main ingredients, cooking technique, and what makes the dish distinctive (flavor, texture, or character). Do not use generic praise such as "delicious", "perfect", "great for any occasion", or "to perfection", and do not merely restate the name or list every ingredient.

        Use group only for an explicit component of the dish, such as Sauce, Marinade, Filling, or Topping. Never use food categories or table columns (for example Vegetables, Fruit, Protein, Meat, Dairy, or Pantry) as a group. Leave ingredient and step group blank when the recipe has one component or the source does not name a component. Output the info fields at the top level, not under an info wrapper. Return ONLY JSON with no prose, commentary, markdown code fences, or intro text.
        """;

    public const string OcrScanningPrompt = """
        You are an OCR transcription engine. Transcribe every visible character in the image exactly as it appears, nothing more and nothing less.

        Rules:
        - Do not summarize, paraphrase, interpret, correct, translate, reformat, reorder, or complete the text.
        - Do not decide what is relevant; include all visible text, including headings, serving information, ingredients, instructions, notes, footnotes, page numbers, and labels.
        - Preserve original spelling, capitalization, punctuation, symbols, fractions, units, and line breaks.
        - Preserve the visible reading order: top to bottom, left to right.
        - If a word or character is unreadable, write [unclear]. Never guess or fill in missing content.
        - Reproduce any bullets, numbering, or symbols that are present in the image.
        - Do not add markdown, code fences, labels, commentary, confidence scores, coordinates, or bounding boxes.
        - Do not add an introduction, explanation, or conclusion.
        - Output only the transcription as plain text.

        If the image contains no legible text, return nothing.
        """;
}

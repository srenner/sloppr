# Standard Recipe

# {{Name}}

{{Description}}

- **Servings:** {{Servings}}
- **Prep Time:** {{PrepMinutes}} mins
- **Cook Time:** {{CookMinutes}} mins
{{#Difficulty}}- **Difficulty:** {{Difficulty}}/3
{{/Difficulty}}

## Ingredients

{{#Ingredients}}
- {{#Quantity}}{{Quantity}} {{/Quantity}}{{#Unit}}{{Unit}} {{/Unit}}{{Name}}{#Group} *({{Group}})*{{/Group}}{#Note} — {{Note}}{{/Note}}
{{/Ingredients}}

## Steps

{{#Steps}}
1. {{#Group}}**[{{Group}}]** {{/Group}}{{Instruction}}
{{/Steps}}

{{#Notes}}
## Notes

{{Notes}}
{{/Notes}}

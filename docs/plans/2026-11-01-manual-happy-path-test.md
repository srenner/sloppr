# Manual test: prompt → meal ideas → pick → recipe → view

Date: 2026-11-01
Status: ready to run
Scope: `sloppr/` backend, run by a human. No Angular UI yet, so "the user" is a REST
client (Scalar, `sloppr.http`, or any HTTP client).
Time: 15–30 minutes, most of it waiting on the local model.

## What is being tested

The first user journey end to end:

1. The user describes what they want to eat.
2. The model proposes meal ideas and the ideas are stored.
3. The user marks one idea as their pick.
4. The system generates a recipe for that idea and stores it.
5. The user can read the recipe as JSON and as markdown.

This is a happy-path check of the API contract, persistence, and ordering. Recipe quality
is not judged here.

## Setup

Automated form: `sloppr/bruno/` is a Bruno collection that runs the steps below and
asserts the same checks. Use this document for setup, expectations, and troubleshooting.

### 1. Runtime

- .NET 10 SDK; repo checked out on `main`.
- Database migrated from `sloppr/`: `dotnet ef database update`.
- Ollama running locally with at least one model pulled, e.g. `ollama pull llama3.1:8b`.
  The model must reliably return JSON; small models often do not.
- API running from `sloppr/`: `dotnet run --launch-profile https`
  (`https://localhost:7187`, Scalar at `/scalar`, plain HTTP on `http://localhost:5220`).
- HTTP client timeout set to at least 180 s. Local generation frequently exceeds 60 s.

### 2. Provider, models, and settings

Skip what is already configured; verify each step first.

| Check | Command | Pass looks like |
|---|---|---|
| Provider exists | `GET /api/AiProviders` | one entry, `providerType: "Ollama"`, `baseUrl` set, `isHealthy: true` |
| Models exist and are active | `GET /api/AiModels` | at least one entry; note its `id` |
| Settings point at a model | `sqlite3 sloppr.db "SELECT ExtractionModelId, IdeaModelId, RecipeModelId FROM ApplicationSettings;"` | three non-null ids of an active model |

If the provider is missing:

```http
POST /api/AiProviders
Content-Type: application/json

{ "name": "Ollama", "providerType": "Ollama", "baseUrl": "http://localhost:11434" }
```

Expect `201`; `422`/`502` mean Ollama is not reachable at that URL.

If models are missing:

```http
GET /api/AiProviders/{providerId}/discover-models
```

Returns minimal model DTOs. Set `"isActive": true` on the ones you want, then:

```http
POST /api/AiModels/batch
Content-Type: application/json

[ { "name": "...", "identifier": "...", "aiProviderId": 1, "isActive": true } ]
```

Expect `200`. Note the new model ids.

Set the three setting ids (there is no settings endpoint):

```bash
cd sloppr
sqlite3 sloppr.db "UPDATE ApplicationSettings
  SET ExtractionModelId = <id>, IdeaModelId = <id>, RecipeModelId = <id>
  WHERE Id = 1;"
```

Restart the API afterwards. `ApplicationSettingService` reads the row once at startup, so
an edit made while the process is running is ignored. If a model id is left null, step 2
returns `500` and step 6 returns `502` (see Troubleshooting).

### 3. Baseline

Record the current recipe count so duplicates are detectable:

```http
GET /api/Recipes
```

## Test steps

Use the prompt below throughout so expectations are concrete.

Prompt: `I have chicken thighs, basmati rice, and a bag of spinach. Something fast for a weeknight.`

| # | Action | Request | Expected | Assert |
|---|---|---|---|---|
| 1 | Describe dinner | `POST /api/AiInference/meal-ideas` `{"prompt":"<prompt>"}` | `200` | body is an array of meal ideas |
| 2 | Inspect ideas | – | – | length is 3 (or at least 1); every item has `id > 0`, non-empty `name`, `isUserApproved: null`, non-empty `keyIngredients` |
| 3 | Check persistence | `GET /api/MealIdeas` | `200` | every id from step 1 is present with the same names |
| 4 | Pick a favourite | note one `ideaId`; `PUT /api/MealIdeas/{ideaId}/approval` `{"isUserApproved":true}` | `204` | response body is empty |
| 5 | Confirm the pick | `GET /api/MealIdeas/{ideaId}` | `200` | `isUserApproved: true`; key ingredients still populated |
| 6 | Generate recipe | `POST /api/AiInference/recipes/{ideaId}` | `201` | `Location` header ends in `/api/Recipes/{recipeId}` |
| 7 | Inspect recipe | – | – | `id > 0`; `mealIdeaId` equals `ideaId`; name and description non-empty; `servingCount >= 1`; prep and cook `>= 0`; `difficulty` 1–3; `formatKey: "standard"`; `formatVersion: 1` |
| 8 | Check ordering | – | – | `ingredients` and `steps` each have at least one row; each `order` is unique and `>= 1`; names/instructions non-empty |
| 9 | Read it back | `GET /api/Recipes/{recipeId}` | `200` | matches step 6 body; ingredients and steps come back in `order` order |
| 10 | Read as markdown | `GET /api/Recipes/{recipeId}/markdown` | `200`, `Content-Type: text/markdown; charset=utf-8` | see markdown checks below |
| 11 | Find it in the list | `GET /api/Recipes` | `200` | the new recipe is present, count is baseline + 1 |
| 12 | Survive a restart | restart the API, repeat step 9 | `200` | same recipe, same ordering |

Markdown checks for step 10:

- starts with `# <recipe name>`;
- description paragraph present;
- info line contains `Servings:`, `Prep:`, `Cook:`, `Difficulty:`;
- `## Ingredients` with one `- ` line per ingredient, quantity/unit/name not mangled;
- `## Steps` with steps numbered from `1.`;
- when any ingredient or step has a group, a `### <group>` heading appears before that
  group; plain recipes have none;
- no stray JSON braces, `\n` escapes, or code fences.

## Known behaviours (not failures)

- Approval is a UX step only. Generating from an unapproved idea still returns `201`;
  repeating step 6 with approval skipped is expected to work today.
- Generating twice for the same idea creates a second recipe. The planned
  meal-idea-to-recipe linkage chunk changes this to `409`; until then, a duplicate is
  expected.
- `formatKey`/`formatVersion` are stamped from the current format row, so they are
  constant for every recipe created by this test.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Step 1 `200` with `[]` | Model returned prose, a wrapper object, or invalid JSON; `LlmArrayResponse` returns an empty list on parse failure | Try a JSON-capable model; check the API console and Ollama logs |
| Step 1 `500` | `ExtractionModelId` or `IdeaModelId` is null; the code calls `.Value` on them | Set both ids, restart the API |
| Step 1 ideas have empty `keyIngredients` | Ingredient extraction returned nothing | Check `ExtractionModelId`; re-run with a prompt that names ingredients |
| Step 6 `502` with "No current recipe format is configured." | `RecipeFormats` has no current row | Re-run `dotnet ef database update`; confirm the seeded row |
| Step 6 `502` with "The recipe model is unavailable or returned an invalid response." | `RecipeModelId` null, model unreachable, or output failed to parse | Check the setting, the provider health, and the model's JSON reliability |
| Step 6 `502` with a list of field errors | Model produced JSON but it violated the format | Re-run; if it repeats, the prompt/model pair needs work |
| Step 6 `404` | Wrong idea id, or the idea was soft-deleted | Use an id from step 3 |
| Request times out | Model loading on first call | Raise the client timeout; retry once |
| Step 4 `400` | `isUserApproved` missing | Send `{"isUserApproved":true}`; `null` clears the vote |

## Result record

Copy this into the run notes:

- Date / tester / API URL:
- Provider and model ids:
- Prompt used:
- Idea ids + picked id + `isUserApproved` after step 5:
- Recipe id + `Location` header:
- Step results: 1–12 pass / fail (attach failing request + full response):
- Anything surprising in the markdown:

Pass = steps 1–11 all match, and step 12 if run. Any non-2xx, missing persistence, wrong
ordering, or broken markdown is a failure.

## Cleanup

Optional, to keep the database tidy:

```http
DELETE /api/Recipes/{recipeId}
DELETE /api/MealIdeas/{ideaId}
```

Both are soft deletes; the rows stay in the database and recipes still render if fetched
by id before deletion. There is no endpoint to reset settings.

## Not covered

- Failure-path matrix (`400`, `404`, `409`, `502`) beyond the troubleshooting table.
- Auth, multi-user behaviour, pagination, and concurrent requests.
- Angular UI: there is no meal-idea or recipe screen yet; this test is REST-only.
- Subjective recipe quality, ingredient realism, and markdown styling in a browser.
- Regeneration/overwrite semantics and duplicate prevention.

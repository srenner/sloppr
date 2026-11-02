# sloppr Bruno collection

Manual end-to-end test of the dinner flow: prompt → meal ideas → approval → recipe →
markdown. It mirrors `docs/plans/2026-11-01-manual-happy-path-test.md`.

## Prerequisites

- API running. `dotnet run --launch-profile https` exposes `http://localhost:5220` and
  `https://localhost:7187`; the `Local` environment points at the HTTP port.
- Database migrated and a current recipe format seeded (`dotnet ef database update`).
- Ollama running with a JSON-capable model pulled.
- An active provider, at least one active model (`isActive: true`), and the three
  `ApplicationSettings` model ids set to that model. Settings load once at startup, so
  restart the API after changing them:

  ```bash
  cd sloppr
  sqlite3 sloppr.db "UPDATE ApplicationSettings
    SET ExtractionModelId = <id>, IdeaModelId = <id>, RecipeModelId = <id>
    WHERE Id = 1;"
  ```

## Run

Bruno app: open this folder as a collection, choose the `Local` environment, then run
`01 Prerequisites` → `02 Meal Ideas` → `03 Recipes`, or the whole collection.

CLI:

```bash
bru run sloppr/bruno --env Local
# or, without a global install:
npx @usebruno/cli run sloppr/bruno --env Local
```

Generate requests have a 180 s timeout; the first call usually includes model load time.

For HTTPS, change `baseUrl` to `https://localhost:7187` and disable SSL verification in
Bruno's preferences (the dev certificate is self-signed).

## Runtime variables

Set automatically as the run progresses:

| Variable | Set by | Used by |
|---|---|---|
| `providerId` | List AI providers | Check provider health |
| `modelId` | List AI models | – (inspect when a request fails) |
| `recipeCountBefore` | Baseline recipes | List recipes |
| `ideaId` | Generate meal ideas | List / approve / get meal idea, generate recipe |
| `ideaName` | Generate meal ideas | – (logged for the run notes) |
| `recipeId` | Generate recipe | Get recipe, markdown, list recipes |
| `recipeName` | Generate recipe | Markdown title check |

Running a single request out of order fails its guard test with a message telling you
which request to run first. To run out of order, set `ideaId` or `recipeId` in the
collection or environment file.

## What it asserts

- Contract shape and status codes for each step.
- Persistence: ideas and recipes are readable through their `GET` routes.
- Approval is stored and read back.
- Recipe fields are within the v1 format limits; ingredient/step orders are unique;
  `GET` returns them sorted; markdown numbers steps from 1.
- Markdown content type and section headings.

Not asserted: recipe quality, ingredient realism, timing, or failure paths.

## Known behaviours

- Approval is a UX step. Generating from an unapproved idea still returns `201`.
- Generating twice for the same idea currently creates a second recipe; `409` arrives
  with the planned meal-idea/recipe linkage chunk.
- Recipes are soft-deleted; `DELETE` does not remove rows.

# LLM endpoint API design

Date: 2026-10-30
Status: complete — implemented in `dfc04df`; open decisions resolved by their defaults
(AiInference kept, provider endpoints unchanged).
Scope: endpoint URLs, HTTP verbs, and parameter location only. No behavior, response-shape, or service changes.

## Goal

Give the endpoints that call a model their own controller instead of living in
single-purpose action controllers, and fix the HTTP semantics while doing it.
The boundary is the request/response contract, not token spend: a route belongs in
the new controller when it neither takes an entity id nor returns an entity resource.

Entity CRUD controllers (`KeyIngredients`, `Cuisines`, `AiModels`, `AiProviders`)
stay as they are.

## Current state

| Verb | Route | Problem |
|---|---|---|
| `POST` | `/api/Chat/idea?prompt=` | prompt in the query string on a POST; controller named for a "chat" concept that no longer exists; path segment singular |
| `GET` | `/api/AiIngredientExtraction/challenge?modelId=` | GET performs N model calls (non-idempotent, spend); controller named after an action; parameter in query |

`ChatController` also injects `IAiModelService`, `IChatClientFactory`, and builds an
unused `List<MealIdeaDTO>`; `AiIngredientExtractionController` injects services it
never uses. Those are not fixed here.

## Decisions

New controller `AiInferenceController` at `[Route("api/[controller]")]`, i.e.
`/api/AiInference`. Multi-segment routes use kebab-case, matching `discover-models`.

| Verb | Route | Request | Response |
|---|---|---|---|
| `POST` | `/api/AiInference/meal-ideas` | body `{ "prompt": "..." }` | `200` + `string[]` |
| `POST` | `/api/AiInference/ingredient-extraction-challenges/{modelId}` | none | `200` + `IngredientExtractionResponse`, `404` when the model is unknown |

Mapping:

| Old | New |
|---|---|
| `POST /api/Chat/idea?prompt=` | `POST /api/AiInference/meal-ideas` |
| `GET /api/AiIngredientExtraction/challenge?modelId=` | `POST /api/AiInference/ingredient-extraction-challenges/{modelId}` |

Rationale and details:

- Both are POST because both are non-idempotent model calls. The challenge endpoint
  in particular must not be a GET: prefetch, retry, or caching middleware could fire it.
- `meal-ideas` is not named `ideas` because the controller no longer supplies domain
  context in its name.
- `challenges` is plural because the action runs every configured
  `ExtractionChallenges` entry and returns a list; `IngredientExtractionResponse.Challenges`
  already reflects that.
- `modelId` is a route segment, not a body field, mirroring
  `POST /api/AiProviders/{id}/health`. Avoids inventing a request DTO for one integer.
- `meal-ideas` needs one new DTO for the prompt (`MealIdeasRequest` in `DTOs/`), since a
  bare `string` cannot bind as a JSON body.
- `200` for `meal-ideas`, not `201`: the response is generated strings, not addressable
  resources, even though `KeyIngredients` and `Cuisines` are upserted as a side effect.
- Response shapes and service calls are unchanged.

## Open decisions

1. **Bucket scope.** `/api/AiInference` holds only the two model-call routes; provider
   health and model discovery stay in `AiProviders`. The wider alternative is a
   non-CRUD bucket (`/api/Ai`) that also absorbs `GET /api/AiProviders/health` and
   `GET /api/AiProviders/{id}/discover-models`. Default: keep `AiInference`, leave
   provider endpoints alone.
2. **`GET /api/AiProviders/health`.** Currently a GET that probes a provider config and
   persists nothing. Flipping it to `POST` with `AiProviderHealthRequest` in the body
   would make it agree with `POST /{id}/health` and get a config object out of the query
   string. Default: leave as-is; it makes no model call and is not broken.
3. **`POST /api/AiProviders/{id}/health`.** Returns a non-entity DTO but writes health
   fields onto the `AiProvider` row. Default: stays in `AiProviders`.

## Implementation steps

1. Add `sloppr/DTOs/MealIdeasRequest.cs` with a required `Prompt` string.
2. Add `sloppr/Controllers/AiInferenceController.cs` with the two actions, moving the
   bodies of `ChatController.GetIdeas` and
   `AiIngredientExtractionController.ExecuteIngredientExtractionAsync` unchanged apart
   from binding (`[FromBody]`, route `modelId`).
3. Delete `sloppr/Controllers/ChatController.cs` and
   `sloppr/Controllers/AiIngredientExtractionController.cs`.
4. Update `sloppr/sloppr.http` with the new routes (prompt in body for `meal-ideas`).
5. Regenerate the Angular client: start the API on `http://localhost:5220`, then
   `npm run codegen` from `sloppr-web/`. Do not hand-edit `sloppr-web/src/app/api/`.
6. Update any hand-written callers of the old routes.

Note: the generated `sloppr-web/src/app/api/chat/chat.service.ts` still targets
`/api/Chat/chat` and `/api/Chat/chat2`, neither of which exists server-side; it is
already stale and will be replaced by step 5.

## Verification

- `dotnet build` in `sloppr/`
- `dotnet run --launch-profile https` in `sloppr/`, then
  `POST /api/AiInference/meal-ideas` and
  `POST /api/AiInference/ingredient-extraction-challenges/{modelId}` (200, and 404 for a
  bad id) via `sloppr.http` or Scalar at `https://localhost:7187/scalar`
- `npm run build` and `npm test -- --watch=false` in `sloppr-web/` after codegen

## Out of scope

- Token spend, caching, streaming, or rate limiting.
- Response shape changes: `meal-ideas` still returns `string[]`, the challenge still
  returns `IngredientExtractionResponse`.
- Unused injected services and the dead `List<MealIdeaDTO>` in the current controllers.
- Health-check-on-create behavior in `POST /api/AiProviders` (returns `422`/`502`).
- Entity CRUD routes under `KeyIngredients`, `Cuisines`, `AiModels`, `AiProviders`.

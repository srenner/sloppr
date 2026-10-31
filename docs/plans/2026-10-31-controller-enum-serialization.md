# Serialize enums as strings in controllers

Date: 2026-10-31
Branch: `fix/controller-enum-serialization`
Scope: `sloppr/Program.cs` only.

## Problem

The app configures JSON twice and only one side was set up.

- `Program.cs` configures `Microsoft.AspNetCore.Http.Json.JsonOptions`. Minimal APIs
  and the **OpenAPI schema generator** read this type
  (`Microsoft.AspNetCore.OpenApi` references `Microsoft.AspNetCore.Http.Json`, not
  `Microsoft.AspNetCore.Mvc.Json`). So `/openapi/v1.json` documents `AiProviderType`
  as `type: string`, and Orval generated a string union in
  `sloppr-web/src/app/api/model/aiProviderType.ts`.
- MVC controllers read `Microsoft.AspNetCore.Mvc.JsonOptions`, which was never
  configured. At runtime the API serializes the enum as `2`.

The published schema and the actual response therefore disagree. The generated client
is typed as a string union, so any consumer comparing against `'Ollama'` silently
fails, and any write round-trips `"Ollama"` into a model that only binds numbers.

Not related to EF Core or SQLite. EF never reads ASP.NET JSON options, and
`AiModel.Capabilities` is a plain string by design (`Models/AiModel.cs:21`,
"SQLite cannot handle JsonDocument natively").

## Decision

Enums travel as strings. Rationale: names are stable under reorder/insert, readable in
logs and `sloppr.http`, and generate typed client unions. The string schema is already
published, so this direction needs no client regeneration; going integer would change
the schema, require `npm run codegen`, and abandon the convention `AGENTS.md` already
documents.

The rule stays global rather than per-enum (`[JsonConverter]` on each enum) so new enums
inherit it without extra ceremony.

## Change

One local helper holds the rule, and both options types are wired from it:

```csharp
builder.Services.Configure<JsonOptions>(o => AddStringEnums(o.SerializerOptions));

builder.Services.AddControllers()
    .AddJsonOptions(o => AddStringEnums(o.JsonSerializerOptions));

// Enum names on the wire, in every serializer this app configures.
static void AddStringEnums(JsonSerializerOptions options) =>
    options.Converters.Add(new JsonStringEnumConverter());
```

Two wiring points are still required, because schema generation and controller output are
driven by two different options types. The helper keeps the conversion rule in one place
if a second converter is added later, and avoids opting in enum-by-enum via
`[JsonConverter]` attributes.

## Verification (human-run)

1. `dotnet build` in `sloppr/`.
2. `dotnet run --launch-profile https`
3. `GET https://localhost:7187/api/AiProviders` → `"providerType": "Ollama"`.
4. `GET https://localhost:7187/openapi/v1.json` → unchanged (`type: string`),
   confirming no codegen churn.

## Out of scope

- Other architecture-sweep findings.
- Response shape or DTO changes.
- `npm run codegen`.

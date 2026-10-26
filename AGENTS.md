# sloppr

Two independent projects with no root solution or manifest. Run commands from each project directory.

- `sloppr/` — ASP.NET Core API (.NET 10, EF Core + SQLite, Microsoft.Extensions.AI/Ollama). Entrypoint: `sloppr/Program.cs`.
- `sloppr-web/` — Angular 21 standalone frontend. Follow `sloppr-web/AGENTS.md` for Angular conventions.
- Root `src/` is an empty leftover; ignore it.

## Backend (`sloppr/`)

```bash
cd sloppr
dotnet build
dotnet run                          # http profile: http://localhost:5220
dotnet run --launch-profile https   # https://localhost:7187 + http://localhost:5220
dotnet ef migrations add <Name>
dotnet ef database update
```

- Use the `https` profile for full-stack work: the frontend proxy targets `https://localhost:7187`.
- Fresh clone: the SQLite DB is gitignored and not auto-migrated. Run `dotnet ef database update` before `dotnet run`, or startup fails loading the seeded single-row `ApplicationSetting`.
- Controllers use `api/[controller]` routes. OpenAPI/Scalar exist only in Development at `/openapi/v1.json` and `/scalar`.
- Repository semantics: `GetFilteredAsync()` applies global query filters; `GetAllAsync()` uses `IgnoreQueryFilters()`.
- DTOs live in `DTOs/` as `ModelNameDTO.cs`; do not modify models for DTO work. Mapping uses Mapperly source generation (`partial` mapper classes).
- Keep `sloppr/sloppr.http` in sync with controller endpoints (see `.continue/prompts/dothttp.md`).
- AI provider support is only Ollama and OpenAI. Anthropic appears in config but throws `NotSupportedException`.
- AI prompts and provider URL paths are configured in `sloppr/appsettings.json`; DB-backed settings are loaded once at startup.
- Enums serialize as strings globally via `JsonStringEnumConverter`.
- `sloppr/_docs/private/` contains gitignored local notes.

## Frontend (`sloppr-web/`)

```bash
cd sloppr-web
npm ci
npm start        # http://localhost:4200, proxies /api to https://localhost:7187
npm run build
npm test -- --watch=false
npx prettier --write .
```

- `src/app/api/` is Orval-generated and tracked. Do not hand-edit it. Regenerate after API contract changes, with the API running at `http://localhost:5220`:
  ```bash
  cd sloppr-web && npm run codegen
  ```
- Generated services call relative `/api/...` paths; development relies on `proxy.conf.json`. `environment.apiUrl` is currently unused.
- Single test (after the stale specs below are fixed): `npx ng test --watch=false --include src/app/path/to/file.spec.ts`.
- No lint script and no CI; `.prettierrc` is the formatter source of truth. There is no backend test project.

## Known test breakage

`npm test` currently fails to compile:

- `src/app/components/ai-models-list/ai-models-list.spec.ts` imports `AiModelsList`, but the class is `AiModelsListComponent`.
- `src/app/app.spec.ts` still expects the old `Hello, sloppr-web` title.

The test tsconfig includes every spec, so `--include`/`--filter` cannot bypass these. Fix the stale specs before trusting test results.

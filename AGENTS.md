# sloppr

Two independent projects, no root solution or manifest. Run commands from each project directory.

- `sloppr/` — ASP.NET Core API (.NET 10, EF Core + SQLite, Microsoft.Extensions.AI/Ollama). Entrypoint: `sloppr/Program.cs`.
- `sloppr-web/` — Angular 21 standalone frontend; follow `sloppr-web/AGENTS.md`.
- Plans live in `docs/plans/` as `<YYYY-MM-DD>-<topic>.md`.

## Agent rules

The AI's role is narrow: **write code and suggest next steps.** Do not act beyond that without being asked.

- **Default:** read/search non-secret files; edit only what the task needs; minimal diffs; end with suggested next steps.
- **Ask first:** any command (build, test, format, run, lint, package managers, git, Docker, scripts); creating/renaming/moving files; config/tooling edits (`*.csproj`, `appsettings*.json`, `angular.json`, `tsconfig*.json`, `proxy.conf.json`, lockfiles, CI, `.vscode/`); dependency changes; refactors or scope creep; test changes.
- **Never:** git writes/history rewrites; migrations; codegen or editing `sloppr-web/src/app/api/`; secrets, DB files, `sloppr/_docs/private/`; deleting files or touching paths outside the repo; weakening/skipping/deleting tests.
- Never claim a command passed unless you ran it; say "not verified" and give the command. Stop and ask if ambiguous.
- Always follow code standards observed within the project. Call out pattern inconsistencies early so they can be corrected.

## Backend (`sloppr/`)

Commands are human-run; suggest them: `dotnet build`, `dotnet run --launch-profile https` (https://localhost:7187, also http://localhost:5220), `dotnet ef database update`.

- Fresh clone: the SQLite DB is gitignored and not auto-migrated; human runs `dotnet ef database update` before `dotnet run`. `dotnet ef migrations add <Name>` is human-only.
- Routes are `api/[controller]`; OpenAPI/Scalar only in Development at `/openapi/v1.json` and `/scalar`.
- Repositories: `GetFilteredAsync()` applies global filters; `GetAllAsync()` uses `IgnoreQueryFilters()`.
- DTOs live in `DTOs/` as `ModelNameDTO.cs`; don't modify models for DTO work; Mapperly source-gen mapping.
- Keep `sloppr/sloppr.http` in sync with controller endpoints.
- Only Ollama/OpenAI are supported (Anthropic throws `NotSupportedException`); prompts/provider URLs are in `appsettings.json`; DB-backed settings load once at startup.
- Enums serialize as strings globally.

## Frontend (`sloppr-web/`)

Commands are human-run; suggest them: `npm ci`, `npm start` (http://localhost:4200, proxies `/api` to https://localhost:7187), `npm run build`, `npm test -- --watch=false`, `npx prettier --write .`.

- `src/app/api/` is Orval-generated; don't hand-edit. After contract changes, human runs `npm run codegen` with the API at `http://localhost:5220`.
- Generated services use relative `/api/...`; dev relies on `proxy.conf.json`; `environment.apiUrl` is unused.
- Single test (after stale specs are fixed): `npx ng test --watch=false --include src/app/path/to/file.spec.ts`.
- No lint script or CI; `.prettierrc` is the formatter source of truth; no backend test project.

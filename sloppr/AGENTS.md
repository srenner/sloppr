# Backend (`sloppr/`)

ASP.NET Core API (.NET 10, EF Core + SQLite, Microsoft.Extensions.AI/Ollama). Entrypoint: `sloppr/Program.cs`.

Commands are human-run; suggest them: `dotnet build`, `dotnet run --launch-profile https` (https://localhost:7187, also http://localhost:5220), `dotnet ef database update`.

- Fresh clone: the SQLite DB is gitignored and not auto-migrated; human runs `dotnet ef database update` before `dotnet run`. `dotnet ef migrations add <Name>` is human-only.
- Routes are `api/[controller]`; OpenAPI/Scalar only in Development at `/openapi/v1.json` and `/scalar`.
- Repositories: `GetFilteredAsync()` applies global filters; `GetAllAsync()` uses `IgnoreQueryFilters()`.
- DTOs live in `DTOs/` as `ModelNameDTO.cs`; don't modify models for DTO work; Mapperly source-gen mapping.
- Keep `sloppr/sloppr.http` in sync with controller endpoints.
- Only Ollama/OpenAI are supported (Anthropic throws `NotSupportedException`); prompts/provider URLs are in `appsettings.json`; DB-backed settings load once at startup.
- Enums serialize as strings globally.
- No backend test project.

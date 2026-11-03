# Markdown Template Engine for Recipes

Date: 2026-11-02
Status: Planned
Scope: `sloppr/` backend markdown rendering endpoint and template loading.

## Goal

Render recipes as markdown using user-generated markdown templates placed in a designated folder. Templates are stored on disk (not in the database) and applied to recipe data.

## Requirements & Design Choices

- **Template Location**: A folder in the application (e.g., `sloppr/Templates/Recipes/` or configured via `appsettings.json`), containing `.md` template files.
- **No Database Storage**: Templates are plain text markdown files managed on disk.
- **Syntax / Engine Options**:
  - Simple mustache/Handlebars-like placeholder replacement (e.g., `{{Name}}`, `{{Description}}`, sections for ingredients and steps).
  - Or a lightweight templating library/custom string interpolation if keeping dependencies minimal.
- **Default Template**: Provide a default template (e.g., `default.md` or `standard.md`) so the system works out-of-the-box without user templates.
- **Endpoint**: Restore `GET /api/Recipes/{id}/markdown` (optionally accepting a template name query parameter, e.g., `?template=default`).

## Planned Work

1. Create a `Templates/Recipes/` folder with a default markdown template (`default.md`).
2. Implement a lightweight template engine / service to load templates from disk, cache them (or reload them in development), and interpolate recipe data (including grouped ingredients and steps).
3. Add back the markdown endpoint `GET /api/Recipes/{id}/markdown` in `RecipesController`.
4. Update `sloppr.http` and tests.

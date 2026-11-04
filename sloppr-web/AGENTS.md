# Frontend (`sloppr-web/`)

Angular 21 standalone frontend.

Commands are human-run; suggest them: `npm ci`, `npm start` (http://localhost:4200, proxies `/api` to https://localhost:7187), `npm run build`, `npm test -- --watch=false`, `npx prettier --write .`.

- `src/app/api/` is Orval-generated; don't hand-edit. After contract changes, human runs `npm run codegen` with the API at `http://localhost:5220`.
- Generated services use relative `/api/...`; dev relies on `proxy.conf.json`; `environment.apiUrl` is unused.
- Single test (after stale specs are fixed): `npx ng test --watch=false --include src/app/path/to/file.spec.ts`.
- No lint script or CI; `.prettierrc` is the formatter source of truth.

## Conventions

- Signals first. Wrap Orval observables with `toSignal(..., { initialValue })`; the async pipe is not used here (see `components/ai-models-list`).
- Types come from the generated `src/app/api/model` barrel. Don't hand-write API interfaces.
- Components use external `templateUrl`/`styleUrl` files, named `foo.ts` / `foo.html` / `foo.css`.
- Folder roles: `pages/` for routed views, `components/` for reusable UI, `layouts/` for shells. Routes live in `app.routes.ts`.
- `.prettierrc`: 100 columns, single quotes, Angular parser for `*.html`. It is the only style enforcement.
- Don't re-add generic Angular style advice here; make it enforceable in `tsconfig.json` or `.prettierrc` instead.

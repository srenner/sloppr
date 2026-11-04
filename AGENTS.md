# sloppr

Two independent projects, no root solution or manifest. Run commands from each project directory.

- Backend — `sloppr/`: ASP.NET Core API. Details: `sloppr/AGENTS.md`.
- Frontend — `sloppr-web/`: Angular 21 standalone. Details: `sloppr-web/AGENTS.md`.

## Workspace boundary

The git root is the only writable workspace. Never read, write, copy, move, or delete
anything outside it — including `/tmp`, `$HOME`, and other absolute paths. This applies to
scratch files, database copies, logs, and test artifacts, not just source edits.

- Keep temporary work under a gitignored directory inside the repo (for example
  `sloppr/obj/scratch/`) and delete it when done.
- Never copy the database to a location outside the repo. If a database copy is required,
  put it under a gitignored directory inside the repo and remove it afterward.
- If a task seems to require an external path, stop and ask before using it.

## Agent rules

The AI's role is narrow: **write code and suggest next steps.** Do not act beyond that without being asked.

- **Default:** read/search non-secret files; edit only what the task needs; minimal diffs; end with suggested next steps.
- **Ask first:** any command (build, test, format, run, lint, package managers, git, Docker, scripts); creating/renaming/moving files; config/tooling edits (`*.csproj`, `appsettings*.json`, `angular.json`, `tsconfig*.json`, `proxy.conf.json`, lockfiles, CI, `.vscode/`); dependency changes; refactors or scope creep; test changes.
- **Never:** git writes/history rewrites; migrations; codegen or editing `sloppr-web/src/app/api/`; secrets, DB files, `sloppr/_docs/private/`; deleting files or touching any path outside the git root; weakening/skipping/deleting tests.
- Never claim a command passed unless you ran it; say "not verified" and give the command. Stop and ask if ambiguous.
- Always follow code standards observed within the project. Call out pattern inconsistencies early so they can be corrected.

### Response style

- Lead with the result or direct answer. No preamble and no restating the request.
- Default to the shortest answer that fully resolves the ask; expand only when the user
  says "explain", "why", or "walk me through".
- Create concise plans with minimal jargon.
- For code changes, report only: files changed, one-line reason, verification command, and
  next step. Do not paste full diffs or file contents unless asked.
- Stop at the requested chunk or step. Do not preview the next one unless asked.
- Ask at most one question per response, and only when the answer changes the
  implementation. Bundle related choices into that question.
- For reviews, start with the verdict ("Yes", "Mostly", "No"), then list only deviations.
- Prefer bullets over paragraphs; use a table only when comparing options.

## Project docs

Read the matching file before touching that project:

- Backend: [`sloppr/AGENTS.md`](sloppr/AGENTS.md) — commands, EF/DB, routing, DTOs, provider support.
- Frontend: [`sloppr-web/AGENTS.md`](sloppr-web/AGENTS.md) — commands, Orval codegen, Angular/TypeScript standards.


<p align="center">
  <img src="sloppr-web/public/favicon.svg" alt="sloppr logo" width="96" height="96">
</p>

# sloppr: it's what's for dinner

## ...What?

sloppr is a self hosted web app that you install on your home network. It uses AI to give you ideas about what to cook for dinner, and helps you maintain a family cookbook.

## How?

.NET, Angular, SQLite, LLMs

## Why?

sloppr can run completely offline with self hosted open weight models. The tech overlords don't need to know what's cooking.

## Building and Running

```bash
docker build -t sloppr .
docker run -d --name sloppr --restart unless-stopped \
  -p 8080:8080 -v sloppr-data:/data sloppr
```

Open `http://localhost:8080`. Put it behind a reverse proxy for
`https://sloppr.local`; the frontend uses relative `/api/...` URLs, so keep the
SPA and API on one origin.

SQLite lives at `/data/sloppr.db`; keep the volume to persist data. Migrations
run at startup, so upgrade by rebuilding and recreating the container. Add your
Ollama endpoint in Settings.

Podman: Docker and Podman keep separate image stores, so build with Podman:
`podman build -t localhost/sloppr .`, then run `localhost/sloppr`. Add `:Z` to
bind mounts on SELinux hosts (`podman machine init && podman machine start` on
macOS/Windows).

No authentication — keep it on your LAN. Run one container only.
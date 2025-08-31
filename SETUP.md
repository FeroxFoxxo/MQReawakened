<div align="center">
    <h1>
        <img width="70%" src="LogoBanner.png?raw=true" style="border-radius: 50%;" align="center">
        <br>
    </h1>
    <h3>Self‑host Setup Guide</h3>
    <h4>This project is completely free from the original game's assets / intellectual property.</h4>
    <h5>None of the repo, the tool, nor the repo owner is affiliated with or sponsored by any affiliates of the original game.</h5>
    <h1></h1>
</div>

## Welcome

If you’re ready to swing into Ook and host your own server emulator, this guide walks you through a clean Docker‑based setup.

## TL;DR

- Place the original client zip under `Game/archives/Client/`
- Place the caches archive (UniqueBundles.7z) under `Game/archives/Caches/`
- Copy over the docker compose file
- Create a `.env` file based on the example env file (all variables you need to edit live there)
- Start with Docker Compose
- Adjust JSON configs under `./Game/data/Configs` after first start

## Prerequisites

- Docker Desktop (or compatible Docker Engine)
- Optional: A domain and an NGINX reverse proxy

## Folder layout used by Docker

- `./Game/archives/Client` → mounted to `/archives/Client` inside the container
- `./Game/archives/Caches` → mounted to `/archives/Caches` inside the container
- `./Game/data` → mounted to `/data` (persisted server data and configs)

## Obtain required files

- Client (zip): Internet Archive link to the original installer download
  - https://archive.org/download/InstallMonkeyQuest/Monkey%20Quest.zip
- Caches: Community archives folder (look for `UniqueBundles.7z`)
  - https://drive.google.com/drive/folders/17ic6S2brJNI9HlFqnue38zFJAv5nqxIU

Place them as follows on the host:

- `./Game/archives/Client/<client>.zip`
- `./Game/archives/Caches/UniqueBundles.7z`

Tip: The entrypoint will automatically extract the latest zip/7z it finds in those folders.

## Configure via .env (edit me first)

All variables you need to edit are in the `.env` file at the repository root. Create it if it doesn’t exist.

```env
# Image source (override if you publish to your own GHCR)
GHCR_OWNER=feroxfoxxo
GHCR_REPO=mqreawakened

# Initial admin account (used on first run only)
DEFAULT_USERNAME=admin
DEFAULT_PASSWORD=admin123
DEFAULT_EMAIL=admin@example.com
DEFAULT_GENDER=Male
DEFAULT_DOB=01-01-2000

# Networking
SERVER_ADDRESS=localhost     # public domain or IP clients should use
GAME_PORT=9339               # TCP game/shard port exposed by container
HTTP_PORT=80                 # web/api port mapped to container

# Data & archives locations (host paths)
DATA_HOST_PATH=./Game/data
GAME_ARCHIVES_PATH=./Game/archives

# Optional toggles
FORCE_REBUILD=0              # set 1 to force clean rebuild/re-extract
SEVEN_Z_THREADS=1            # 7z CPU threads; 1 is conservative on memory
```

Notes:

- `SERVER_ADDRESS` should be the public name clients will reach (domain or IP). This is written into various config files and URLs the client consumes.
- `FORCE_REBUILD=1` clears build output and cached data on next start (use when changing DLLs or archives).
- `SEVEN_Z_THREADS=1` is recommended on low‑memory systems when extracting large caches.

## Start the server (Docker Compose)

From the repository root:

```bash
docker compose up -d
```

What happens:

- The entrypoint builds and publishes the app if needed
- It extracts the client DLLs from the client zip (once)
- It extracts caches from `UniqueBundles.7z` (once)
- It synchronizes default assets to `/data`
- It starts the server on port 80 (HTTP) and `GAME_PORT` (TCP)

Visit your server at `http://localhost` (or your domain) and check `/healthz` for a simple health check.

## Reverse proxy (NGINX)

The client will not work over modern HTTPS unless you force an older TLS configuration. To keep things simple, ensure you also expose a plain HTTP server. Here’s an example NGINX config to proxy both HTTPS and HTTP to your container:

```nginx
server {
    listen 443 ssl;
    listen [::]:443 ssl;

    server_name mqr.example.com;

    location / {
        set $upstream_app IP; # (replace me)
        set $upstream_port HTTP_PORT; # (replace me)
        set $upstream_proto http;

        proxy_pass $upstream_proto://$upstream_app:$upstream_port;
    }
}

server {
    listen 80;
    listen [::]:80;

    server_name mqr.example.com;

    location / {
        set $upstream_app IP; # (replace me)
        set $upstream_port HTTP_PORT; # (replace me)
        set $upstream_proto http;

        proxy_pass $upstream_proto://$upstream_app:$upstream_port;
    }
}
```

Replace `IP` and `HTTP_PORT` with your Docker host and the host port you mapped to container `:80` (commonly `${HTTP_PORT}`), or point to a Docker network alias if NGINX runs in the same compose network.

## Post‑start configuration (JSON under /data/Configs)

After the first run, JSON configuration files are available under the host path `./Game/data/Configs` (inside the container: `/data/Configs`). Edit them and restart the container to apply changes.

Commonly adjusted files:

- `Internal.json` (from `InternalRwConfig`)
  - `ServerName`: Display name for your server
  - `ServerAddress`: Hostname or IP
  - `Port`: Game TCP port (usually `9339`)
  - `IsHttps`: Whether generated URLs should use https (client prefers http)
  - `DiscordServerId`: Optional server/community widget id

- `Discord.json` (from `DiscordRwConfig`)
  - `DiscordBotToken`, `ChannelId`, `ReportsChannelId`: for in-game player reporting

- `Website.json` (from `WebsiteRwConfig`)
  - `SmtpServer`, `SmtpPort`, `SmtpUser`, `SmtpPass`, `SenderAddress`: for password resets

- `Server.json` (from `ServerRwConfig`)
  - `CurrentEventOverride`, `CurrentTimedEventOverride`: for switching the current in-game event

These JSONs are auto‑created with sensible defaults if missing. You can safely version‑control the `./Game/data/Configs` folder in your own environment if you like.

## Troubleshooting

- Missing client zip or DLLs
  - Ensure a client `.zip` is present under `./Game/archives/Client/` and contains a `game/…/Managed/` folder with DLLs like `UnityEngine.dll` and `Assembly-CSharp.dll`.
- Caches didn’t extract
  - Put `UniqueBundles.7z` under `./Game/archives/Caches/` and restart. If memory‑constrained, set `SEVEN_Z_THREADS=1`.
- Need a clean rebuild
  - Set `FORCE_REBUILD=1` in `.env` and start again.

## Happy hosting — and see you in Ook.

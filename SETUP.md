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

> [!IMPORTANT]
> Since we aren't modifying the installer, you should start the game from ``/pa/pa.exe`` (the patcher) directly. Most clients come with an InstallMQ.exe and uninstall.exe file that you may want to remove from the archive to not confuse players before uploading it to your server - adding a text file to direct them to run ``/pa/pa.exe``.

## TL;DR

- Copy over the [docker compose](https://github.com/FeroxFoxxo/MQReawakened/blob/main/compose.yaml) file
- Create a `.env` file based on the [example env file](https://github.com/FeroxFoxxo/MQReawakened/blob/main/.env.example) (all variables you need to edit live there)
- Place the original client zip under `./Build/Archives/Client/...` (alternatively use the mount you detailed in the env file)
- Place the caches archive (UniqueBundles.7z) under `./Build/Archives/Caches/...` (alternatively use the mount you detailed in the env file)
- Start the Docker Compose file
- Adjust JSON configs under `./Game/Data/Configs` after first start

## Folder layout used by Docker

- `./Build/Archives/Client` → mounted to `/archives/Client` inside the container
  - **2014 client archive** (required): Contains DLLs needed for server compilation
  - **Override client archive** (optional): Alternative game client (i.e. 2013)
- `./Build/Archives/Caches` → mounted to `/archives/Caches` inside the container
- `./Game/Data` → mounted to `/data` (persisted server data and configs)

## Prerequisites

- Docker Desktop (or compatible Docker Engine) with [Docker Compose](https://docs.docker.com/compose/install/linux/)
- Optional: A domain and an NGINX reverse proxy

## Download MQReawakened docker files

- .env example: [View](https://github.com/FeroxFoxxo/MQReawakened/blob/main/.env.example) • [Download raw](https://raw.githubusercontent.com/FeroxFoxxo/MQReawakened/main/.env.example)
  - Save as `.env` in the folder you plan to use for the server
- Docker Compose: [View](https://github.com/FeroxFoxxo/MQReawakened/blob/main/compose.yaml) • [Download raw](https://raw.githubusercontent.com/FeroxFoxxo/MQReawakened/main/compose.yaml)
  - Save as `compose.yaml` in the folder

## Obtain required game files

**Important**: This server can use **two different client versions**:

1. **2014 Client (Required for server compilation)**: Contains the DLLs needed to compile the server
2. **2013 Client (Optional for game hosting)**: Can be used for actual game hosting if preferred

### Client Downloads

- **2014 Client (Required)**: Any client archive that contains the required 2014 version DLLs (see [here](https://archive.org/download/InstallMonkeyQuest/Monkey%20Quest.zip))
- **Override Client (Optional)**: Any alternative client archive for hosting (see [here](https://drive.google.com/drive/folders/1AuNMaNqbszUzWBgT3d_xolSuH_IINJf))
- **Caches**: Any/all caches you have from the original game, i.e. to view the community archives - look for `UniqueBundles.7z` [here](https://drive.google.com/drive/folders/17ic6S2brJNI9HlFqnue38zFJAv5nqxIU)

### File Placement

Place the client archives as follows in your server folder:

- `./Build/Archives/Client/` - Place your 2014 client archive here (any .zip or .7z file) - required as used for compilation of the server
- `./Build/Archives/ClientOverride/` - Place alternative client archive here (optional, any .zip or .7z file) - i.e. 2013
- `./Build/Archives/Caches/UniqueBundles.7z`

**Note**: The 2014 client is **mandatory** as it provides the DLLs needed for server compilation. The override client is optional and will only be used for game hosting if present in the `ClientOverride` folder.

**Automatic Fallback**: If no override client is found in the `ClientOverride` folder, the system will automatically use the 2014 client for both dependencies and hosting.

Tip: The docker entrypoint will automatically extract the latest zip/7z it finds in those folders.

## Configure via .env

All variables you need to edit are in the `.env` file. Create it if it doesn't exist.

### Example .env Configuration

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
DATA_HOST_PATH=./Game/Data
GAME_ARCHIVES_PATH=./Build/Archives

# Optional toggles
FORCE_REBUILD=0              # set 1 to force clean rebuild/re-extract
SEVEN_Z_THREADS=1            # 7z CPU threads; 1 is conservative on memory
```

### Client Version Behavior

The system automatically detects and uses the best available client version:
- **2014 client is always required** for server compilation (provides DLLs)
- **Override client is automatically used for hosting** if present in the `ClientOverride` folder
- **2014 client is used for hosting** if no override client is found
- **No configuration needed** - the system chooses automatically based on folder structure

Notes:

- `SERVER_ADDRESS` should be the public name clients will reach (domain or IP). This is written into various config files and URLs the client consumes.
- `FORCE_REBUILD=1` clears build output and cached data on next start.
- `SEVEN_Z_THREADS=1` is recommended on low‑memory systems when extracting large caches.
- **Client version selection is automatic**: The system will use override client for hosting if available, otherwise fall back to 2014 client.

## Start the server (Docker Compose)

From the repository root:

```bash
docker compose up -d
```

What happens:

- The entrypoint extracts the **2014 client** first to obtain required DLLs for server compilation
- It **automatically detects** which client version to use for game hosting:
  - If **override client is found** in `ClientOverride` folder, it uses that for hosting
  - If **no override client is found**, it automatically uses the 2014 client for hosting
- It extracts caches from `UniqueBundles.7z` (once)
- It synchronizes default assets to `/data`
- It starts the server on port 80 (HTTP) and `GAME_PORT` (TCP)

Visit your server at `http://localhost` (or your domain) and check `/healthz` for a simple health check.

## Update the container (Docker)

Keeping up to date is simple and non‑destructive. Your data and configs live under `./Game/Data` and are mounted into the container, so pulling a new image will not wipe them.

Typical update flow:

```bash
# 1) Fetch the latest image(s)
docker compose pull

# 2) Recreate and start with the new version
docker compose up -d

# 3) (Optional) Watch logs to ensure a clean start
docker compose logs -f --tail=100 mqreawakened
```

Notes:

- Clean rebuild/extract: set `FORCE_REBUILD=1` in `.env`, then `docker compose up -d`. Set it back to `0` afterwards to avoid repeated rebuilds.
- Breaking change recovery: if something seems stuck after an update, try `docker compose down` followed by `docker compose up -d`.
- Compose file changes: if the upstream `compose.yaml` changes, re‑download it and merge any local edits (like ports) before running the steps above.

## Reverse proxy (NGINX)

The client will not work over modern HTTPS unless you use an older TLS version. To keep things simple, ensure you also expose a plain HTTP server. Here's an example NGINX config to proxy both HTTPS and HTTP to your container:

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

After the first run, the JSON configuration files are available under the host path `./Game/data/Configs` (inside the container: `/data/Configs`). Edit them and restart the container to apply changes.

Commonly adjusted files:

- `Internal.json` (from `InternalRwConfig`)
  - `ServerName`: Display name for your server
  - `ServerAddress`: Hostname or IP
  - `Port`: Game TCP port (usually `9339`)
  - `IsHttps`: Whether generated URLs should use https (game client uses http)
  - `DiscordServerId`: Optional server/community widget id

- `Discord.json` (from `DiscordRwConfig`)
  - `DiscordBotToken`, `ChannelId`, `ReportsChannelId`: for in-game player reporting

- `Website.json` (from `WebsiteRwConfig`)
  - `SmtpServer`, `SmtpPort`, `SmtpUser`, `SmtpPass`, `SenderAddress`: for password resets

- `Server.json` (from `ServerRwConfig`)
  - `CurrentEventOverride`, `CurrentTimedEventOverride`: for switching the current in-game event

These JSONs are auto‑created with sensible defaults if missing. You can safely version‑control the `./Game/data/Configs` folder in your own environment if you like.

## Troubleshooting

- Missing 2014 client or DLLs
  - Ensure a client archive (.zip or .7z) is present under `./Build/Archives/Client/` and contains a `game/…/Managed/` folder with DLLs like `UnityEngine.dll` and `Assembly-CSharp.dll`.
  - The 2014 client is **required** for server compilation regardless of which version you want to host.
- Missing override client for hosting
  - If you want to use an alternative client for hosting, ensure it's present under `./Build/Archives/ClientOverride/`.
  - If no override client is found, the system will automatically use the 2014 client for hosting.
- Caches didn't extract
  - Put `UniqueBundles.7z` under `./Build/Archives/Caches/` and restart. If memory‑constrained, set `SEVEN_Z_THREADS=1`.
- Need a clean rebuild
  - Set `FORCE_REBUILD=1` in `.env` and start again.
- Server restarting in Docker
  - Run ``docker logs mqreawakened``. If still confusing, forward these logs into the Discord.
- Patcher/launcher cannot retrieve patch information
  - Make sure the domain is set correctly in the .env file
- Game loading bar does not fill
  - Make sure the game port 9339 is port forwarded
- Cannot click on "Play" in the launcher
  - Open the game first to pass Windows smart-screen, then retry.
- Game crashes after loading into Clock Tower Square
  - You're likely using an Intel IGPU. There is no fix for this.
- Game stops loading after playing for a while
  - Clear the cache files in LocalLow

## Happy hosting — and see you in Ook.

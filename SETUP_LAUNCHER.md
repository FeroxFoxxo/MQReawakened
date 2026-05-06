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

If you’re ready to swing into Ook and host your own server emulator, this guide walks you through a clean Powershell‑based setup.

> [!IMPORTANT]
> Since we aren't modifying the installer, you should start the game from ``/pa/pa.exe`` (the patcher) directly. Most clients come with an InstallMQ.exe and uninstall.exe file that you may want to remove from the archive to not confuse players before uploading it to your server - adding a text file to direct them to run ``/pa/pa.exe``.

> [!NOTE]
> You can also watch this video guide: https://youtu.be/QB06PacS_0A

## TL;DR

- Download the [MQRLauncher](https://github.com/FeroxFoxxo/MQReawakened/releases/latest) file
- Download the [MQClient](https://github.com/FeroxFoxxo/MQClient/releases/latest) file (this is used for playing the 2012 versions)
- Create a `.env` file based on the [example env file](https://github.com/FeroxFoxxo/MQReawakened/blob/main/.env.example) (all variables you need to edit live there)
- Place the original client zip under `MQData/Build/Archives/Client/...`
- Place the caches archive (UniqueBundles.7z) under `MQData/Build/Archives/Caches/...`
- Start the Play MQReawakened file
- Adjust JSON configs under `MQData/Game/Data/Configs` after first start

## Download MQReawakened files

- .env example: [View](https://github.com/FeroxFoxxo/MQReawakened/blob/main/.env.example) • [Download raw](https://raw.githubusercontent.com/FeroxFoxxo/MQReawakened/main/.env.example)
  - Save as `.env` in the folder you plan to use for the server
- Download the [MQRLauncher](https://github.com/FeroxFoxxo/MQReawakened/releases/latest)
- Download the [MQClient](https://github.com/FeroxFoxxo/MQClient/releases/latest)
  - This is used for the 2012 version

## Obtain required game files

### Client Information 

**Important**: This server can use **three different client versions**:

1. **2014 Client (Required for server compilation)**: Contains the DLLs needed to compile the server
2. **2013 Client (Optional for playing the game)**: Version of the game you want to play on, defaults to what's used to compile the server (2014)
3. **2012 WebPlayer Client (Optional for playing the game)**: You can find these inside of the 2012 zip on the Override Client archive below.

### External Downloads

- **2014 Client (Required)**: Any client archive that contains the required 2014 version DLLs (see [here](https://archive.org/download/InstallMonkeyQuest/Monkey%20Quest.zip))
- **Override Client (Optional)**: Any alternative client archive for the version actually played on (see [here](https://drive.google.com/drive/folders/17twM6SsVxRLgd6ZgL7UvVNO8S0ckHX7a))
- **Caches**: Any/all caches you have from the original game, i.e. to view the community archives - look for `UniqueBundles.7z` [here](https://drive.google.com/file/d/1jwM79M5JguVPqCvkvocPtEmE3ArieT3Y/view)

### File Placement

Place the client archives as follows in your server folder:

- `MQData/Build/Archives/Client/` - Place your 2014 client archive here (any .zip or .7z file) - required as used for compilation of the server
- `MQData/Build/Archives/ClientOverride/` - Place alternative client archive here (optional, any .zip or .7z file) - i.e. 2013
- `MQData/Build/Archives/Caches/UniqueBundles.7z`

**Note**: The 2014 client is **mandatory** as it provides the DLLs needed for server compilation. The override client is optional and will only be used for game hosting if present in the `ClientOverride` folder.

**Automatic Fallback**: If no override client is found in the `ClientOverride` folder, the system will automatically use the 2014 client for both dependencies and hosting.

Tip: The entrypoint will automatically extract the latest zip/7z it finds in those folders.

## Configure via .env

All variables you need to edit are in the `.env` file. Create it if it doesn't exist.

### Example .env Configuration

```env
# Repo source (override if you publish to your own github)
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

# Optional toggles
FORCE_REBUILD=0              # set 1 to force clean rebuild/re-extract
SEVEN_Z_THREADS=1            # 7z CPU threads; 1 is conservative on memory
```

Notes:

- `SERVER_ADDRESS` should be the public name clients will reach (domain or IP). This is written into various config files and URLs the client consumes.
- `FORCE_REBUILD=1` clears build output and cached data on next start.
- `SEVEN_Z_THREADS=1` is recommended on low‑memory systems when extracting large caches.
- `GAME_PORT` is not recommended to change as the client expects this port.

## Start the server

From the root folder run the Play MQReawakened file.

What happens:

- The entrypoint extracts the **2014 client** first to obtain required DLLs for server compilation
- It **automatically detects** which client version to use for game hosting:
  - If **override client is found** in `ClientOverride` folder, it uses that for hosting
  - If **no override client is found**, it automatically uses the 2014 client for hosting
- It extracts caches from `UniqueBundles.7z` (once)
- It synchronizes default assets to `/data`
- It starts the server on port 80 (HTTP) and `GAME_PORT` (TCP)

Visit your server at `http://localhost` and check `/healthz` for a simple health check.

## Update the container

Keeping up to date is simple and non‑destructive. Your data and configs live under `MQData/Game/Data` and are mounted into the container, so pulling changes will not wipe them.

Updates are automatic whenever you restart the container it will check the github repo for any new changes.

Notes:

- Clean rebuild/extract: set `FORCE_REBUILD=1` in `.env`, then run Play MQReawakened. Set it back to `0` afterwards to avoid repeated rebuilds.

## Post‑start configuration (JSON under /data/Configs)

After the first run, the JSON configuration files are available under the host path `MQData/Game/data/Configs`. Edit them and restart the container to apply changes.

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

These JSONs are auto‑created with sensible defaults if missing. You can safely version‑control the `MQData/Game/data/Configs` folder in your own environment if you like.

## Troubleshooting

- Missing 2014 client or DLLs
  - Ensure a client archive (.zip or .7z) is present under `MQData/Build/Archives/Client/` and contains a `game/…/Managed/` folder with DLLs like `UnityEngine.dll` and `Assembly-CSharp.dll`.
  - The 2014 client is **required** for server compilation regardless of which version you want to host.
- Missing override client for hosting
  - If you want to use an alternative client for hosting, ensure it's present under `MQData/Build/Archives/ClientOverride/`.
  - If no override client is found, the system will automatically use the 2014 client for hosting.
- Caches didn't extract
  - Put `UniqueBundles.7z` under `MQData/Build/Archives/Caches/` and restart. If memory‑constrained, set `SEVEN_Z_THREADS=1`.
- Need a clean rebuild
  - Set `FORCE_REBUILD=1` in `.env` and start again.
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
- File extraction and/or asset bundle transversal taking a long time
  - Generally this should take ~30m depending on your system.

## Happy playing — and see you in Ook.

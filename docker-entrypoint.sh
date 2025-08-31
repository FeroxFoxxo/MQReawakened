#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
OUT_DIR="$APP_DIR/out"
SOLUTION_FILE="$APP_DIR/MQReawaken.sln"
INIT_PROJ="$APP_DIR/Init/Init.csproj"
DEPS_DIR="$APP_DIR/Server.Reawakened/Dependencies"
SETTINGS_DIR="/settings"
CLIENT_ARCHIVES_DIR="/archives/Client"
CACHES_ARCHIVES_DIR="/archives/Caches"
CACHES_DIR="/data/Caches"

sync_dir() {
  local src="$1"; local dest="$2"; local label="$3"
  if [[ -d "$src" ]]; then
    echo "[entrypoint] Syncing directory: $label -> $dest"
    rm -rf "$dest"
    mkdir -p "$dest"
    cp -a "$src/." "$dest/"
  else
    echo "[entrypoint] WARNING: Missing source dir for $label at $src"
  fi
}

sync_file() {
  local src="$1"; local dest="$2"; local label="$3"
  local dest_dir
  dest_dir="$(dirname "$dest")"
  if [[ -f "$src" ]]; then
    echo "[entrypoint] Syncing file: $label -> $dest"
    mkdir -p "$dest_dir"
    cp -f "$src" "$dest"
  else
    echo "[entrypoint] WARNING: Missing source file for $label at $src"
  fi
}

echo "[entrypoint] Starting container for MQReawakened"

if [[ "${FORCE_REBUILD:-0}" == "1" ]]; then
  echo "[entrypoint] FORCE_REBUILD=1, clearing previous build output and caches"
  rm -rf "$OUT_DIR"
  rm -rf "/data/Caches"
fi

need_prepare=false

if [[ "${FORCE_REBUILD:-0}" == "1" ]]; then need_prepare=true; fi
if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then need_prepare=true; fi
if ! compgen -G "$DEPS_DIR/*.dll" > /dev/null; then need_prepare=true; fi

if [[ "$need_prepare" == "true" ]]; then
  echo "[entrypoint] Preparing game files from archive in $CLIENT_ARCHIVES_DIR"
  if [[ ! -d "$CLIENT_ARCHIVES_DIR" ]]; then
    echo "[entrypoint] WARNING: Client archives directory not found at $CLIENT_ARCHIVES_DIR"
  fi
  latest_zip="$(find "$CLIENT_ARCHIVES_DIR" -type f -name '*.zip' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
  if [[ -z "$latest_zip" || ! -f "$latest_zip" ]]; then
    if compgen -G "$DEPS_DIR/*.dll" > /dev/null; then
      echo "[entrypoint] No game zip found in $CLIENT_ARCHIVES_DIR, but dependencies exist. Skipping extraction."
    else
      echo "[entrypoint] ERROR: No game zip found in $CLIENT_ARCHIVES_DIR and no dependencies present at $DEPS_DIR. Cannot build."
      echo "[entrypoint] Please place a game .zip under $CLIENT_ARCHIVES_DIR on the host (mapped to /archives/Client)."
      exit 1
    fi
  else
    echo "[entrypoint] Using game archive: $latest_zip"
    rm -rf "$SETTINGS_DIR"
    mkdir -p "$SETTINGS_DIR"
    unzip -oq "$latest_zip" -d "$SETTINGS_DIR"

    if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then
      top_children=("$SETTINGS_DIR"/*)
      if [[ ${#top_children[@]} -eq 1 && -d "${top_children[0]}" ]]; then
        echo "[entrypoint] Flattening extracted directory structure"
        tmp_dir="${top_children[0]}"
        shopt -s dotglob
        cp -a "$tmp_dir"/* "$SETTINGS_DIR"/
        shopt -u dotglob
        rm -rf "$tmp_dir"
      fi
    fi

    if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then
      echo "[entrypoint] WARNING: settings.txt not found in extracted archive at $SETTINGS_DIR"
    fi

    mkdir -p "$DEPS_DIR"
    game_root="$SETTINGS_DIR/game"
    if [[ ! -d "$game_root" ]]; then
      echo "[entrypoint] ERROR: Expected 'game' directory under '/settings' not found after extraction."
      exit 1
    fi
    managed_dir="$(find "$game_root" -type d -name Managed -print -quit 2>/dev/null || true)"
    if [[ -z "$managed_dir" || ! -d "$managed_dir" ]]; then
      echo "[entrypoint] ERROR: Could not locate a 'Managed' directory under '$game_root'"
      exit 1
    fi
    if ! compgen -G "$managed_dir/*.dll" > /dev/null; then
      echo "[entrypoint] ERROR: No DLLs found in '$managed_dir'"
      exit 1
    fi
    if [[ ! -f "$managed_dir/UnityEngine.dll" || ! -f "$managed_dir/Assembly-CSharp.dll" ]]; then
      echo "[entrypoint] ERROR: Required game DLLs not found in '$managed_dir' (expected UnityEngine.dll and Assembly-CSharp.dll)"
      exit 1
    fi
    echo "[entrypoint] Copying DLLs from $managed_dir to $DEPS_DIR"
    cp -f "$managed_dir"/*.dll "$DEPS_DIR"/
  fi
fi

mkdir -p "$CACHES_DIR"
if [[ ! -d "$CACHES_ARCHIVES_DIR" ]]; then
  echo "[entrypoint] WARNING: Caches archives directory not found at $CACHES_ARCHIVES_DIR"
fi
if [[ ! -f "$CACHES_DIR/__info" ]]; then
  cache_archive=""
  if compgen -G "$CACHES_ARCHIVES_DIR/*.7z" > /dev/null; then
    cache_archive="$(find "$CACHES_ARCHIVES_DIR" -type f -name '*.7z' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
  elif compgen -G "$CACHES_ARCHIVES_DIR/*.zip" > /dev/null; then
    cache_archive="$(find "$CACHES_ARCHIVES_DIR" -type f -name '*.zip' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
  fi

  if [[ -n "$cache_archive" ]]; then
    echo "[entrypoint] Extracting caches from $cache_archive to $CACHES_DIR"
    case "$cache_archive" in
      *.7z)
        echo "[entrypoint] 7z extraction in progress..."
        7z x -bsp1 -bso0 -y -o"$CACHES_DIR" "$cache_archive" ;;
      *.zip)
        printf "[entrypoint] unzip in progress"
        unzip -oq "$cache_archive" -d "$CACHES_DIR" &
        unzip_pid=$!
        while kill -0 "$unzip_pid" 2>/dev/null; do printf "."; sleep 1; done
        wait "$unzip_pid"
        printf "\n" ;;
    esac
    top_children=("$CACHES_DIR"/*)
    if [[ ${#top_children[@]} -eq 1 && -d "${top_children[0]}" ]]; then
      tmp_dir="${top_children[0]}"
      shopt -s dotglob
      cp -a "$tmp_dir"/* "$CACHES_DIR"/
      shopt -u dotglob
      rm -rf "$tmp_dir"
    fi
  else
    echo "[entrypoint] No caches archive (.7z or .zip) found in $CACHES_ARCHIVES_DIR. If first run, place a caches archive there."
  fi
fi

if [[ ! -f "$CACHES_DIR/__info" ]]; then
  echo "[entrypoint] Creating caches marker file at $CACHES_DIR/__info"
  : > "$CACHES_DIR/__info"
fi

did_build=0
if [[ ! -f "$OUT_DIR/Init.dll" ]]; then
  echo "[entrypoint] Build output not found, building application..."
  dotnet restore "$SOLUTION_FILE"
  dotnet publish "$INIT_PROJ" -c Release -o "$OUT_DIR"
  did_build=1
else
  echo "[entrypoint] Build output found. Skipping build."
fi

if [[ "$did_build" == "1" || "${FORCE_REBUILD:-0}" == "1" ]]; then
  SRC_LOCAL_ASSETS="$APP_DIR/Server.Reawakened/Assets/LocalAssets"
  DEST_ASSETS_BASE="/data/Assets"
  sync_dir "$SRC_LOCAL_ASSETS" "$DEST_ASSETS_BASE/LocalAssets" "LocalAssets"

  SRC_LICENSES="$APP_DIR/Server.Reawakened/Licences"
  DEST_LICENSES="/data/Licences"
  sync_dir "$SRC_LICENSES" "$DEST_LICENSES" "Licences"

  SRC_THRIFT_NOTICE="$APP_DIR/Server.Reawakened/Thrift/NOTICE"
  DEST_THRIFT_DIR="/data/Thrift/NOTICE"
  sync_file "$SRC_THRIFT_NOTICE" "$DEST_THRIFT_DIR" "Thrift NOTICE"

  DOWNLOADS_DIR="/data/Downloads"
  if compgen -G "$DOWNLOADS_DIR/*.zip" > /dev/null; then
    echo "[entrypoint] Removing existing download zip(s) in $DOWNLOADS_DIR due to build/update"
    rm -f "$DOWNLOADS_DIR"/*.zip
  fi
fi

echo "[entrypoint] Launching application..."
cd "$OUT_DIR"
exec dotnet "Init.dll"

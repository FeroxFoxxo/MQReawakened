#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
OUT_DIR="$APP_DIR/out"
SOLUTION_FILE="$APP_DIR/MQReawaken.sln"
INIT_PROJ="$APP_DIR/Init/Init.csproj"
DEPS_DIR="$APP_DIR/Server.Reawakened/Dependencies"
SETTINGS_DIR="/settings"
WIN_GAME_DIR="/archives"

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
  echo "[entrypoint] FORCE_REBUILD=1, clearing previous build output"
  rm -rf "$OUT_DIR"
fi

need_prepare=false

if [[ "${FORCE_REBUILD:-0}" == "1" ]]; then need_prepare=true; fi
if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then need_prepare=true; fi
if ! compgen -G "$DEPS_DIR/*.dll" > /dev/null; then need_prepare=true; fi

if [[ "$need_prepare" == "true" ]]; then
  echo "[entrypoint] Preparing game files from archive in $WIN_GAME_DIR"
  latest_zip="$(find "$WIN_GAME_DIR" -type f -name "*.zip" -print0 2>/dev/null | xargs -0 ls -1t 2>/dev/null | head -n 1 || true)"
  if [[ -z "$latest_zip" ]]; then
    if compgen -G "$DEPS_DIR/*.dll" > /dev/null; then
      echo "[entrypoint] No game zip found in $WIN_GAME_DIR, but dependencies exist. Skipping extraction."
    else
      echo "[entrypoint] ERROR: No game zip found in $WIN_GAME_DIR and no dependencies present at $DEPS_DIR. Cannot build."
      echo "[entrypoint] Please place a game .zip under $WIN_GAME_DIR on the host (mapped to /archives)."
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
    managed_dir="$(find "$SETTINGS_DIR" -type d -name Managed 2>/dev/null | head -n 1 || true)"
    if [[ -z "$managed_dir" ]]; then
      echo "[entrypoint] ERROR: Could not locate Managed in extracted archive"
      exit 1
    fi
    if ! compgen -G "$managed_dir/*.dll" > /dev/null; then
      echo "[entrypoint] ERROR: Managed directory at $managed_dir contains no DLLs"
      exit 1
    fi
    echo "[entrypoint] Copying DLLs from $managed_dir to $DEPS_DIR"
    cp -f "$managed_dir"/*.dll "$DEPS_DIR"/
  fi
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
fi

echo "[entrypoint] Launching application..."
cd "$OUT_DIR"
exec dotnet "Init.dll"

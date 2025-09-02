#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
OUT_DIR="$APP_DIR/out"
SOLUTION_FILE="$APP_DIR/MQReawaken.sln"
INIT_PROJ="$APP_DIR/Init/Init.csproj"
DEPS_DIR="$APP_DIR/Server.Reawakened/Dependencies"
SETTINGS_DIR="/settings"
CLIENT_ARCHIVES_DIR="/archives/Client"
CLIENT_OVERRIDE_DIR="/archives/ClientOverride"
CACHES_ARCHIVES_DIR="/archives/Caches"
CACHES_DIR="/data/Caches"
# Control 7z threads to reduce memory usage; can be overridden via env
SEVEN_Z_THREADS="${SEVEN_Z_THREADS:-1}"

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
  echo "[entrypoint] Preparing game files from archives in $CLIENT_ARCHIVES_DIR"
  if [[ ! -d "$CLIENT_ARCHIVES_DIR" ]]; then
    echo "[entrypoint] WARNING: Client archives directory not found at $CLIENT_ARCHIVES_DIR"
  fi
  
  # First, find and extract 2014 client for dependencies (required for server compilation)
  echo "[entrypoint] Looking for 2014 client for server dependencies..."
  client_2014_zip=""
  if compgen -G "$CLIENT_ARCHIVES_DIR/*.zip" > /dev/null; then
    client_2014_zip="$(find "$CLIENT_ARCHIVES_DIR" -type f -name '*.zip' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
  elif compgen -G "$CLIENT_ARCHIVES_DIR/*.7z" > /dev/null; then
    client_2014_zip="$(find "$CLIENT_ARCHIVES_DIR" -type f -name '*.7z' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
  fi
  
  if [[ -z "$client_2014_zip" ]]; then
    echo "[entrypoint] ERROR: No 2014 client found. Server requires 2014 DLLs for compilation."
    echo "[entrypoint] Please place a 2014 client archive (*.zip or *.7z) under $CLIENT_ARCHIVES_DIR"
    exit 1
  fi
  
  echo "[entrypoint] Using 2014 client for dependencies: $client_2014_zip"
  
  # Extract 2014 client to temporary location for DLLs
  temp_2014_dir="/tmp/client_2014_deps"
  rm -rf "$temp_2014_dir"
  mkdir -p "$temp_2014_dir"
  
  case "$client_2014_zip" in
    *.7z)
      echo "[entrypoint] Extracting 2014 client from 7z archive..."
      7z x -mmt="$SEVEN_Z_THREADS" -bso0 -bse0 -y -o"$temp_2014_dir" "$client_2014_zip" >/dev/null
      ;;
    *.zip)
      echo "[entrypoint] Extracting 2014 client from zip archive..."
      unzip -oq "$client_2014_zip" -d "$temp_2014_dir" >/dev/null
      ;;
  esac
  
  # Flatten directory structure if needed
  top_children=("$temp_2014_dir"/*)
  if [[ ${#top_children[@]} -eq 1 && -d "${top_children[0]}" ]]; then
    echo "[entrypoint] Flattening 2014 client directory structure"
    tmp_dir="${top_children[0]}"
    shopt -s dotglob
    cp -a "$tmp_dir"/* "$temp_2014_dir"/
    shopt -u dotglob
    rm -rf "$tmp_dir"
  fi
  
  # Extract DLLs from 2014 client
  mkdir -p "$DEPS_DIR"
  game_root_2014="$temp_2014_dir/game"
  if [[ ! -d "$game_root_2014" ]]; then
    echo "[entrypoint] ERROR: Expected 'game' directory not found in 2014 client"
    exit 1
  fi
  
  managed_dir_2014="$(find "$game_root_2014" -type d -name Managed -print -quit 2>/dev/null || true)"
  if [[ -z "$managed_dir_2014" || ! -d "$managed_dir_2014" ]]; then
    echo "[entrypoint] ERROR: Could not locate 'Managed' directory in 2014 client"
    exit 1
  fi
  
  if ! compgen -G "$managed_dir_2014/*.dll" > /dev/null; then
    echo "[entrypoint] ERROR: No DLLs found in 2014 client Managed directory"
    exit 1
  fi
  
  if [[ ! -f "$managed_dir_2014/UnityEngine.dll" || ! -f "$managed_dir_2014/Assembly-CSharp.dll" ]]; then
    echo "[entrypoint] ERROR: Required game DLLs not found in 2014 client (expected UnityEngine.dll and Assembly-CSharp.dll)"
    exit 1
  fi
  
  echo "[entrypoint] Copying DLLs from 2014 client to $DEPS_DIR"
  cp -f "$managed_dir_2014"/*.dll "$DEPS_DIR"/
  
  # Now check for client override in the Override subfolder
  echo "[entrypoint] Checking for client override in $CLIENT_OVERRIDE_DIR..."
  hosting_client_zip=""
  hosting_version=""
  
  # Check for override client in the Override subfolder
  if [[ -d "$CLIENT_OVERRIDE_DIR" ]] && (compgen -G "$CLIENT_OVERRIDE_DIR/*.zip" > /dev/null || compgen -G "$CLIENT_OVERRIDE_DIR/*.7z" > /dev/null); then
    if compgen -G "$CLIENT_OVERRIDE_DIR/*.zip" > /dev/null; then
      hosting_client_zip="$(find "$CLIENT_OVERRIDE_DIR" -type f -name '*.zip' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
    elif compgen -G "$CLIENT_OVERRIDE_DIR/*.7z" > /dev/null; then
      hosting_client_zip="$(find "$CLIENT_OVERRIDE_DIR" -type f -name '*.7z' -printf '%T@\t%p\n' 2>/dev/null | sort -nr | head -n 1 | cut -f2-)"
    fi
    hosting_version="override"
    echo "[entrypoint] Found override client, using it for hosting: $hosting_client_zip"
  else
    # No override found, use 2014 client for hosting
    hosting_client_zip="$client_2014_zip"
    hosting_version="2014"
    echo "[entrypoint] No override client found, using 2014 client for hosting"
  fi
  
  # Extract the hosting client
  echo "[entrypoint] Using $hosting_version client for game hosting: $hosting_client_zip"
  rm -rf "$SETTINGS_DIR"
  mkdir -p "$SETTINGS_DIR"
  
  case "$hosting_client_zip" in
    *.7z)
      echo "[entrypoint] Extracting $hosting_version client for game hosting..."
      7z x -mmt="$SEVEN_Z_THREADS" -bso0 -bse0 -y -o"$SETTINGS_DIR" "$hosting_client_zip" >/dev/null
      ;;
    *.zip)
      echo "[entrypoint] Extracting $hosting_version client for game hosting..."
      unzip -oq "$hosting_client_zip" -d "$SETTINGS_DIR" >/dev/null
      ;;
  esac
  
  # Flatten directory structure if needed
  if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then
    top_children=("$SETTINGS_DIR"/*)
    if [[ ${#top_children[@]} -eq 1 && -d "${top_children[0]}" ]]; then
      echo "[entrypoint] Flattening $hosting_version client directory structure"
      tmp_dir="${top_children[0]}"
      shopt -s dotglob
      cp -a "$tmp_dir"/* "$SETTINGS_DIR"/
      shopt -u dotglob
      rm -rf "$tmp_dir"
    fi
  fi
  
  if [[ ! -f "$SETTINGS_DIR/settings.txt" ]]; then
    echo "[entrypoint] WARNING: settings.txt not found in extracted $hosting_version client at $SETTINGS_DIR"
  fi
  
  # Clean up temporary 2014 extraction
  rm -rf "$temp_2014_dir"
else
  echo "[entrypoint] Dependencies already present, skipping client extraction"
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
        printf "[entrypoint] 7z extraction in progress"
        7z x -mmt="$SEVEN_Z_THREADS" -bso0 -bse0 -y -o"$CACHES_DIR" "$cache_archive" >/dev/null &
        z_pid=$!
        while kill -0 "$z_pid" 2>/dev/null; do printf "."; sleep 1; done
        if ! wait "$z_pid"; then
          status=$?
          echo "\n[entrypoint] ERROR: 7z extraction failed with exit code $status"
          if [[ "$status" == "137" ]]; then
            echo "[entrypoint] Hint: The container was likely OOMKilled. Try setting SEVEN_Z_THREADS=1 (default) or increasing container memory."
          fi
          exit "$status"
        fi
        printf "\n" ;;
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

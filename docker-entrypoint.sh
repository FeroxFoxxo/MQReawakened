#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
OUT_DIR="$APP_DIR/out"
SOLUTION_FILE="$APP_DIR/MQReawaken.sln"
INIT_PROJ="$APP_DIR/Init/Init.csproj"
DEPS_DIR="$APP_DIR/Server.Reawakened/Dependencies"

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

wait_timeout=60
elapsed=0

while [[ $elapsed -lt $wait_timeout ]]; do
  if compgen -G "$DEPS_DIR/*.dll" > /dev/null; then
    echo "[entrypoint] Found dependency DLLs in $DEPS_DIR"
    break
  fi
  echo "[entrypoint] Waiting for dependency DLLs to be mounted in $DEPS_DIR... ($elapsed/${wait_timeout}s)"
  sleep 1
  elapsed=$((elapsed+1))
done

if ! compgen -G "$DEPS_DIR/*.dll" > /dev/null; then
  echo "[entrypoint] WARNING: No DLLs found in $DEPS_DIR after ${wait_timeout}s. Proceeding to build anyway."
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

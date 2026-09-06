#!/usr/bin/env bash
# macOS standalone build. Usage: scripts/build-mac.sh [--dev]
#
# Worth having separately from the web build: it runs at native speed with no browser
# focus rules in the way, which is the difference between "the frame time looks fine"
# and knowing it does. WebGL is still the thing that ships.
set -euo pipefail

UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.2.15f1/Unity.app/Contents/MacOS/Unity}"
PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/Mac"
LOG="$PROJECT/Build/build-mac.log"

[ -x "$UNITY" ] || { echo "No Unity at $UNITY. Set UNITY=<path>."; exit 1; }

if pgrep -f "Unity.app/Contents/MacOS/Unity" >/dev/null 2>&1; then
  echo "Close the Unity editor first, it holds the project lock."
  exit 1
fi

DEV=""
[ "${1:-}" = "--dev" ] && DEV="-dev"

mkdir -p "$PROJECT/Build"
echo "Building macOS to $OUT (log: $LOG)"

"$UNITY" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod MacBuild.BuildFromCommandLine \
  -out "$OUT" $DEV \
  -logFile "$LOG" -accept-apiupdate

grep -E "BUILD (OK|FAILED|START)" "$LOG" || true

APP="$(find "$OUT" -maxdepth 1 -name '*.app' 2>/dev/null | head -1)"
if [ -n "$APP" ]; then
  echo
  echo "Run it with:  open '$APP'"
  echo "Unsigned, so the first launch needs right click > Open, or:"
  echo "  xattr -dr com.apple.quarantine '$APP'"
fi

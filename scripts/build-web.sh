#!/usr/bin/env bash
# WebGL build. Usage: scripts/build-web.sh [--dev]
set -euo pipefail

UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.2.15f1/Unity.app/Contents/MacOS/Unity}"
PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
LOG="$PROJECT/Build/build-web.log"

[ -x "$UNITY" ] || { echo "No Unity at $UNITY. Set UNITY=<path>."; exit 1; }

# Unity refuses to open a project a second time, and the failure is easy to misread.
if pgrep -f "Hub/Editor/.*MacOS/Unity -projectpath $PROJECT" >/dev/null; then
  echo "Close the Unity editor first, it holds the project lock."
  exit 1
fi

DEV=""
[ "${1:-}" = "--dev" ] && DEV="-dev"

mkdir -p "$PROJECT/Build"
echo "Building WebGL to $OUT (log: $LOG)"

"$UNITY" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod WebBuild.BuildFromCommandLine \
  -out "$OUT" $DEV \
  -logFile "$LOG" -accept-apiupdate

grep -E "BUILD (OK|FAILED|START)" "$LOG" || true
echo "Done. Open $OUT/index.html through a web server, not file://"

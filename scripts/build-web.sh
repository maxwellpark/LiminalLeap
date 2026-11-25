#!/usr/bin/env bash
# WebGL build. Usage: scripts/build-web.sh [--dev]
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source-path=SCRIPTDIR source=unity-path.sh
. "$PROJECT/scripts/unity-path.sh"
OUT="$PROJECT/Build/WebGL"
LOG="$PROJECT/Build/build-web.log"

[ -x "$UNITY" ] || { echo "No Unity at $UNITY. Set UNITY=<path>."; exit 1; }

# Unity refuses to open a project a second time, and the failure is easy to misread.
if unity_has_project_open "$PROJECT"; then
  echo "Close the Unity editor first, it holds the project lock."
  exit 1
fi

DEV=""
[ "${1:-}" = "--dev" ] && DEV="-dev"

mkdir -p "$PROJECT/Build"
echo "Building WebGL to $OUT (log: $LOG)"

"${UNITY_RUN[@]}" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod WebBuild.BuildFromCommandLine \
  -out "$OUT" $DEV \
  -logFile "$LOG" -accept-apiupdate

grep -E "BUILD (OK|FAILED|START)" "$LOG" || true
echo "Done. Open $OUT/index.html through a web server, not file://"

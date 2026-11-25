#!/usr/bin/env bash
# WebGL build, held to the art budget. Usage: scripts/build-web.sh [--dev]
# WEB_BUDGET_MB overrides the 25 MB limit for one run.
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

# The generated scenes aren't committed, and the build lists them. Same seeds as check.sh --full.
echo "Generating scenes (log: $PROJECT/Build/scenes.log)"
rm -rf "$PROJECT/Assets/Scenes/Generated"
"${UNITY_RUN[@]}" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod TestSceneGenerator.GenerateFromCommandLine -seed 1700 -count 3 \
  -logFile "$PROJECT/Build/scenes.log" -accept-apiupdate \
  || { echo "Scene generation failed, see Build/scenes.log"; exit 1; }

echo "Building WebGL to $OUT (log: $LOG)"

"${UNITY_RUN[@]}" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod WebBuild.BuildFromCommandLine \
  -out "$OUT" $DEV \
  -logFile "$LOG" -accept-apiupdate

grep -E "BUILD (OK|FAILED|START)" "$LOG" || true

# Measures the shipped folder; dev builds carry debug data, so only release is held to it.
BUDGET_MB="${WEB_BUDGET_MB:-25}"
SIZE_KB=$(du -sk "$OUT" | cut -f1)
if [ -z "$DEV" ] && [ "$SIZE_KB" -gt $((BUDGET_MB * 1024)) ]; then
  echo "Over budget: $((SIZE_KB / 1024)) MB, and the limit is $BUDGET_MB MB (docs/ART.md). The largest assets:"
  sed -n '/^Used Assets and files/,/^$/p' "$LOG" | head -12
  exit 1
fi
echo "Size: $((SIZE_KB / 1024)) MB of $BUDGET_MB MB"
echo "Done. Open $OUT/index.html through a web server, not file://"

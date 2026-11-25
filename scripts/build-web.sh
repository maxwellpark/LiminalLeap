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

# The generated scenes aren't committed, and the build lists them. Same seeds as check.sh --full.
echo "Generating scenes (log: $PROJECT/Build/scenes.log)"
rm -rf "$PROJECT/Assets/Scenes/Generated"
"${UNITY_RUN[@]}" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod TestSceneGenerator.GenerateFromCommandLine -seed 1700 -count 3 \
  -logFile "$PROJECT/Build/scenes.log" -accept-apiupdate \
  || { echo "Scene generation failed, see Build/scenes.log"; exit 1; }
# Unity can log a failed generation and still exit 0, so count the scenes it reports.
SCENES=$(grep -c '^GENERATED ' "$PROJECT/Build/scenes.log" || true)
[ "$SCENES" -eq 3 ] || { echo "Generated $SCENES of 3 scenes, see Build/scenes.log"; exit 1; }

echo "Building WebGL to $OUT (log: $LOG)"

"${UNITY_RUN[@]}" -batchmode -quit -projectPath "$PROJECT" \
  -executeMethod WebBuild.BuildFromCommandLine \
  -out "$OUT" $DEV \
  -logFile "$LOG" -accept-apiupdate

grep -E "BUILD (OK|FAILED|START)" "$LOG" || true
echo "Done. Open $OUT/index.html through a web server, not file://"

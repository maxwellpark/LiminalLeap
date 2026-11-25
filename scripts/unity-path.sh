# shellcheck shell=bash
# shellcheck disable=SC2034  # read by the scripts that source this
# Sourced: UNITY, its UNITY_TOOLS folder, and UNITY_RUN, which adds a display when headless.

if [ "$(uname)" = "Darwin" ]; then
  UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.2.15f1/Unity.app/Contents/MacOS/Unity}"
  UNITY_TOOLS="$(dirname "$(dirname "$UNITY")")"
else
  UNITY="${UNITY:-/opt/unity/6000.2.15f1/Editor/Unity}"
  UNITY_TOOLS="$(dirname "$UNITY")/Data"
fi

UNITY_RUN=("$UNITY")
if [ "$(uname)" != "Darwin" ] && [ -z "${DISPLAY:-}" ] && command -v xvfb-run >/dev/null 2>&1; then
  UNITY_RUN=(xvfb-run -a -s "-screen 0 1280x720x24" "$UNITY")
fi

# True while an editor has this project open, which holds the lock batch mode needs.
unity_has_project_open() {
  # Escaped and anchored, so a path inside this one, or with regex characters, can't match.
  local path
  path=$(printf '%s' "${1%/}" | sed 's/[][\.^$*+?(){}|/]/\\&/g')
  pgrep -if -- "-projectpath ${path}( |$)" >/dev/null 2>&1
}

# Batch mode exits 198 when there's no licence, which otherwise reads as a missing result.
unity_licence_hint() {
  [ "$1" = "198" ] && echo "   no Unity licence on this machine: scripts/unity-activate.sh (docs/SETUP.md)"
  return 0
}

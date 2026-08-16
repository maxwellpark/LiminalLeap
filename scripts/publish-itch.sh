#!/usr/bin/env bash
# Push the WebGL build to itch.io. Usage: scripts/publish-itch.sh [user/game:channel]
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
TARGET="${1:-${ITCH_TARGET:-maxwellpark/liminal-leap:html5}}"

[ -d "$OUT" ] || { echo "No build at $OUT. Run scripts/build-web.sh first."; exit 1; }
[ -f "$OUT/index.html" ] || { echo "$OUT has no index.html, build looks incomplete."; exit 1; }

if ! command -v butler >/dev/null 2>&1; then
  cat <<'EOF'
butler is not installed.

  brew install butler          # or download from https://itch.io/docs/butler/
  butler login                 # opens a browser, stores credentials

CI instead wants BUTLER_API_KEY set, from https://itch.io/user/settings/api-keys
EOF
  exit 1
fi

# butler reads BUTLER_API_KEY when set, otherwise the credentials from butler login.
if [ -z "${BUTLER_API_KEY:-}" ] && [ ! -f "$HOME/.config/itch/butler_creds" ]; then
  echo "Not logged in. Run 'butler login' or set BUTLER_API_KEY."
  exit 1
fi

VERSION="$(git -C "$PROJECT" rev-parse --short HEAD)"
echo "Pushing $OUT -> $TARGET (version $VERSION)"

butler push "$OUT" "$TARGET" --userversion "$VERSION"
butler status "$TARGET"

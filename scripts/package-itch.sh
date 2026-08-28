#!/usr/bin/env bash
# Zips the WebGL build for manual upload to itch. Usage: scripts/package-itch.sh
#
# butler push takes a directory, so publishing does not need this. It exists for the
# drag and drop path on the itch site, which needs no CLI credentials at all.
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
DIST="$PROJECT/Build/dist"
VERSION="$(date -u +%Y%m%d-%H%M)"
ZIP="$DIST/liminal-leap-web-$VERSION.zip"

[ -f "$OUT/index.html" ] || { echo "No build at $OUT. Run scripts/build-web.sh first."; exit 1; }

mkdir -p "$DIST"
rm -f "$ZIP"

# Zipped from inside the build directory: itch needs index.html at the root of the
# archive, not nested under a folder, or it serves a directory listing instead of a game.
(cd "$OUT" && zip -qr "$ZIP" . -x '.DS_Store' -x '__MACOSX/*')

echo "Wrote $ZIP"
echo
echo "Top of the archive (index.html must be here, not in a subfolder):"
unzip -l "$ZIP" | head -12
echo
echo "On itch: upload it, tick 'This file will be played in the browser',"
echo "and set the viewport wide. The template scales to whatever frame it is given."

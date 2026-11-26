#!/usr/bin/env bash
# The itch page's images from the current web build: Build/store/shot-*.png and cover.png.
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
STORE="$PROJECT/Build/store"

[ -f "$OUT/index.html" ] || { echo "No build at $OUT: run scripts/build-web.sh first." >&2; exit 2; }
NODE_PATH="${NODE_PATH:-$(npm root -g 2>/dev/null || true)}"
export NODE_PATH

# Made beside the old images and swapped in at the end, so a failed run leaves them as they were.
NEXT="$(mktemp -d "$PROJECT/Build/store-next.XXXXXX")"
trap 'rm -rf "$NEXT"' EXIT
node "$PROJECT/scripts/web-run.js" "$OUT" "$NEXT/shots" --size=1920x1200
node "$PROJECT/scripts/web-run.js" "$OUT" "$NEXT/cover" --size=630x500 --only=daily-school

# itch wants three to five screenshots: the story and each place's daily, mid-run.
n=1
for run in story daily-office daily-school daily-house; do
  cp "$NEXT/shots/$run-run.png" "$NEXT/shot-$n.png"
  n=$((n + 1))
done
cp "$NEXT/cover/daily-school-run.png" "$NEXT/cover.png"

rm -rf "$STORE"
mv "$NEXT" "$STORE"
trap - EXIT
echo "Images in $STORE: shot-1.png to shot-4.png, and cover.png"

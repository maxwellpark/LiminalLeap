#!/usr/bin/env bash
# The web build in a headless browser: web-check's boot, then every run. Usage: scripts/check-web.sh [--build]
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
CAPTURES="$PROJECT/Build/captures"

# shipwright's tags are what consumers pin; web-check runs from this one, not the checkout's head.
SHIPWRIGHT_TAG=v0.6.0

if [ "${1:-}" = "--build" ]; then
  "$PROJECT/scripts/build-web.sh"
fi
[ -f "$OUT/index.html" ] || { echo "No build at $OUT: run scripts/build-web.sh, or pass --build."; exit 2; }

SHIPWRIGHT="${SHIPWRIGHT:-$PROJECT/../shipwright}"
git -C "$SHIPWRIGHT" rev-parse -q --verify "$SHIPWRIGHT_TAG^{commit}" >/dev/null 2>&1 \
  || { echo "No shipwright $SHIPWRIGHT_TAG at $SHIPWRIGHT: clone it there and fetch its tags, or set SHIPWRIGHT."; exit 2; }
command -v node >/dev/null 2>&1 || { echo "Node isn't installed."; exit 2; }

# Playwright's own Chromium, unless one is named, so both checks use the same browser.
NODE_PATH="${NODE_PATH:-$(npm root -g 2>/dev/null || true)}"
export NODE_PATH
BROWSER="${SHIPWRIGHT_BROWSER:-$(node -e "try { console.log(require('playwright').chromium.executablePath()) } catch (e) {}")}"
if [ -z "$BROWSER" ] || [ ! -x "$BROWSER" ]; then
  echo "No browser: npm i -g playwright@1.63.0 && npx playwright@1.63.0 install chromium, or set SHIPWRIGHT_BROWSER."
  exit 2
fi

PINNED="$(mktemp -d)"
trap 'rm -rf "$PINNED"' EXIT
git -C "$SHIPWRIGHT" archive "$SHIPWRIGHT_TAG" bin lib | tar -x -C "$PINNED"

mkdir -p "$CAPTURES"
echo "== boot"
SHIPWRIGHT_BROWSER="$BROWSER" python3 "$PINNED/bin/web-check" "$OUT" --ready-marker --mute --screenshot "$CAPTURES/boot.png"
echo "== runs"
node "$PROJECT/scripts/web-run.js" "$OUT" "$CAPTURES"

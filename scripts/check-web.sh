#!/usr/bin/env bash
# The web build in a headless browser: shipwright's web-check for the boot, then scripts/web-run.js
# for a story run and each place's daily. Usage: scripts/check-web.sh [--build]
# Needs shipwright beside this repo (or SHIPWRIGHT=<path>), Node and Playwright with Chromium.
set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$PROJECT/Build/WebGL"
CAPTURES="$PROJECT/Build/captures"

[ "${1:-}" = "--build" ] && "$PROJECT/scripts/build-web.sh"
[ -f "$OUT/index.html" ] || { echo "No build at $OUT: run scripts/build-web.sh, or pass --build."; exit 2; }

SHIPWRIGHT="${SHIPWRIGHT:-$PROJECT/../shipwright}"
[ -x "$SHIPWRIGHT/bin/web-check" ] || { echo "No shipwright at $SHIPWRIGHT; set SHIPWRIGHT=<path>."; exit 2; }
command -v node >/dev/null 2>&1 || { echo "Node isn't installed."; exit 2; }

# Playwright's own Chromium, unless one is named, so both checks use the same browser.
BROWSER="${SHIPWRIGHT_BROWSER:-$(node -e "try { console.log(require('playwright').chromium.executablePath()) } catch (e) {}")}"
[ -n "$BROWSER" ] || { echo "No browser: npm i -g playwright && npx playwright install chromium, or set SHIPWRIGHT_BROWSER."; exit 2; }

mkdir -p "$CAPTURES"
echo "== boot"
SHIPWRIGHT_BROWSER="$BROWSER" "$SHIPWRIGHT/bin/web-check" "$OUT" --ready-marker --mute --screenshot "$CAPTURES/boot.png"
echo "== runs"
node "$PROJECT/scripts/web-run.js" "$OUT" "$CAPTURES"

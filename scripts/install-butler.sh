#!/usr/bin/env bash
# Install itch.io's butler CLI. Usage: scripts/install-butler.sh
#
# Not `brew install butler`: that cask is Butler by Many Tricks, a macOS launcher app
# with nothing to do with itch.io. itch's butler is not in Homebrew at all, it ships
# from itch's own CDN.
#
# butler loads sibling libraries (7z.so, libc7zip.dylib) from next to the binary, so the
# whole payload goes in one directory and only the binary is linked onto PATH.
set -euo pipefail

DEST="${BUTLER_BIN:-$HOME/.local/bin}"
LIB="${BUTLER_LIB:-$HOME/.local/lib/butler}"

case "$(uname -s)-$(uname -m)" in
  Darwin-arm64)  CHANNEL="darwin-arm64" ;;
  Darwin-x86_64) CHANNEL="darwin-amd64" ;;
  Linux-x86_64)  CHANNEL="linux-amd64" ;;
  *) echo "No known butler channel for $(uname -s)-$(uname -m)"; exit 1 ;;
esac

URL="https://broth.itch.zone/butler/$CHANNEL/LATEST/archive/default"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

echo "Fetching butler for $CHANNEL"
curl -fsSL -o "$TMP/butler.zip" "$URL"
unzip -o -q "$TMP/butler.zip" -d "$TMP"

[ -f "$TMP/butler" ] || { echo "Archive did not contain a butler binary."; exit 1; }

mkdir -p "$DEST" "$LIB"
cp "$TMP/butler" "$TMP"/*.so "$TMP"/*.dylib "$LIB/" 2>/dev/null || cp "$TMP/butler" "$LIB/"
ln -sf "$LIB/butler" "$DEST/butler"

# Downloaded binaries are quarantined on macOS and refuse to run until cleared.
xattr -dr com.apple.quarantine "$LIB" 2>/dev/null || true

echo "Installed: $("$DEST/butler" -V 2>&1 | head -1)"

case ":$PATH:" in
  *":$DEST:"*) ;;
  *) echo; echo "$DEST is not on PATH. Add it:"; echo "  echo 'export PATH=\"\$HOME/.local/bin:\$PATH\"' >> ~/.zshrc" ;;
esac

echo
echo "Next:  butler login"

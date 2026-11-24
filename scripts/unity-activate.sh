#!/usr/bin/env bash
# Activate this machine's Unity with the automation account in op/unity.env, via 1Password.
# shellcheck disable=SC2016
#
# Usage:
#   scripts/unity-activate.sh            activate, falling back to Personal if there's no seat
#   scripts/unity-activate.sh --check    confirm the references resolve, without revealing them
#   scripts/unity-activate.sh --status   what this machine is entitled to now
#   scripts/unity-activate.sh --return   give the activation back, before a machine is thrown away
#
# The licensing client only takes the login as arguments, so it shows in ps while it runs.
set -euo pipefail

SCRIPTS="$(cd "$(dirname "$0")" && pwd)"
ENV_FILE="$SCRIPTS/../op/unity.env"

# shellcheck source-path=SCRIPTDIR source=unity-path.sh
. "$SCRIPTS/unity-path.sh"
ROOT="$(dirname "$UNITY_TOOLS")"

# It ships inside the editor, at a different depth on each platform.
CLIENT="$(find "$ROOT" -type f -name Unity.Licensing.Client -perm -u+x 2>/dev/null | head -1)"
[ -n "$CLIENT" ] || { echo "No licensing client under $ROOT. Set UNITY=<path to the editor binary>."; exit 1; }

if [ "${1:-}" = "--status" ]; then
  exec "$CLIENT" --showEntitlements
fi

if ! command -v op >/dev/null 2>&1; then
  echo "1Password CLI not installed. See scripts/deploy-itch.sh for setup; a service account"
  echo "token in OP_SERVICE_ACCOUNT_TOKEN is the unattended route."
  exit 1
fi

[ -f "$ENV_FILE" ] || { echo "Missing $ENV_FILE"; exit 1; }

case "${1:-}" in
  --check)
    # Lengths only. Never the value or any prefix of it.
    op run --env-file="$ENV_FILE" -- sh -c '
      ok=0
      for name in UNITY_EMAIL UNITY_PASSWORD; do
        eval "value=\${$name:-}"
        if [ -n "$value" ]; then echo "$name resolved, ${#value} chars"; else echo "$name EMPTY"; ok=1; fi
      done
      exit $ok'
    ;;
  --return)
    op run --env-file="$ENV_FILE" -- sh -c \
      '"$0" --deactivate-all --username "$UNITY_EMAIL" --password "$UNITY_PASSWORD"' "$CLIENT"
    ;;
  "")
    # Single quoted, so the values expand in op's child and never in this shell.
    op run --env-file="$ENV_FILE" -- sh -c \
      '"$0" --activate-all --include-personal --username "$UNITY_EMAIL" --password "$UNITY_PASSWORD"' "$CLIENT"
    "$CLIENT" --showEntitlements
    ;;
  *)
    echo "Unknown option: $1"
    exit 1
    ;;
esac

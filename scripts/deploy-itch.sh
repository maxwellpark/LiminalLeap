#!/usr/bin/env bash
# Push the WebGL build to itch with the API key resolved from 1Password at run time.
#
# Usage:
#   scripts/deploy-itch.sh            push using the reference in scripts/itch.env
#   scripts/deploy-itch.sh --check    confirm the reference resolves, without revealing it
#
# The point of this wrapper: the key is never written to disk, never in the repo, and
# never in this shell's environment. op injects it into the child process only, and masks
# it if it appears in the child's output.
#
# Honest about the boundary: this stops accidental disclosure and casual reading. It is
# not a sandbox. Anything that can read its own environment can transform the value and
# print it, so masking is a strong default rather than a guarantee. The real containment
# is the service account below being scoped to one vault holding one revocable key.
set -euo pipefail

SCRIPTS="$(cd "$(dirname "$0")" && pwd)"
ENV_FILE="$SCRIPTS/itch.env"

if ! command -v op >/dev/null 2>&1; then
  cat <<'EOF'
1Password CLI not installed.

  brew install 1password-cli

Then either:
  - Desktop app: enable Settings > Developer > "Integrate with 1Password CLI".
    Interactive use, unlocks with Touch ID.
  - Unattended or agent use: create a service account scoped to a vault that holds
    only the itch key, and export its token:
      export OP_SERVICE_ACCOUNT_TOKEN=...
    Scoped to one vault so the blast radius is one credential, revocable at itch.io.
EOF
  exit 1
fi

[ -f "$ENV_FILE" ] || { echo "Missing $ENV_FILE"; exit 1; }

if [ "${1:-}" = "--check" ]; then
  # Prints whether it resolved and how long it is. Never the value, and never a prefix
  # of it, because a prefix of a credential is still part of a credential.
  op run --env-file="$ENV_FILE" -- \
    sh -c 'if [ -n "${BUTLER_API_KEY:-}" ]; then echo "resolved, ${#BUTLER_API_KEY} chars"; else echo "EMPTY"; exit 1; fi'
  exit $?
fi

exec op run --env-file="$ENV_FILE" -- "$SCRIPTS/publish-itch.sh" "$@"

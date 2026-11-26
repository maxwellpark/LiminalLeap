# Acceptance: max/frame-copy

The daily's share line copies inside a frame like itch's, which grants no clipboard permission.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Copy works in a cross-origin frame with no clipboard permission | `web-run.js --only=frame-copy`: the office's daily in an iframe from another origin, the Copy button clicked, the clipboard read back from the parent starts "Liminal Leap daily" | pass | verified (browser) |
| 2 | It's the textarea fallback that does it there | the run records which way the copy went; it requires "fallback" (the async clipboard is refused in the frame) | pass | verified (browser) |
| 3 | Every other run still passes | `scripts/check-web.sh --build`: eleven runs | pass | verified |

## Unverified

- itch's real iframe attributes, which could grant clipboard-write; then the async path copies,
  which `daily-copy` checks.

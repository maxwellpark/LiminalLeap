# Acceptance: max/daily-copy

The daily's share line copies from the summary's Copy button in a real browser.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A click on Copy puts the share line on the clipboard | `web-run.js --only=daily-copy`: the office's daily left to end, the button clicked at the rectangle the page marks, `navigator.clipboard.readText()` starts "Liminal Leap daily" and holds the itch link | pass | verified (browser) |
| 2 | The summary says so | `Build/captures/daily-copy.png`: the button reads COPIED | pass | verified (browser) |
| 3 | The check can't pass without the bridge | on the build before, the page marks no copy target and the run times out | fail as expected | verified (browser) |
| 4 | Every other run still passes | `scripts/check-web.sh --build`: ten runs | pass | verified |

## Unverified

- The textarea fallback, for browsers that refuse the async clipboard; Chromium grants it here.
- Inside itch's frame, where the clipboard needs the iframe's `allow="clipboard-write"`, which
  is itch's to set.

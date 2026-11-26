# Acceptance: max/save-reload

The save lives in the browser's storage and survives a reload.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A fresh browser starts with no daily runs | `web-run.js --only=save-reload`: the title marks `data-daily-runs` 0 | pass | verified (browser) |
| 2 | A daily played to its end is still counted after a reload | the same run: the office's daily left to end, the page reloaded in the same browser, the title marks at least 1 | pass | verified (browser) |
| 3 | The check catches a title that doesn't mark it | on the build before: "counts null daily runs" | fail as expected | verified (browser) |
| 4 | Every other run still passes | `scripts/check-web.sh --build`: twelve runs | pass | verified |

## Unverified

- A reload within the first second after a run ends: the run waits 2 s for the asynchronous
  storage write. A player closing the tab that fast could lose that run's record.
- Private windows and browsers that clear storage on exit.

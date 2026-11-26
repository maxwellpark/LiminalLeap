# Acceptance: max/page-marks

The page marks the title, each run and each lap apart, for browser checks.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The title marks the page `data-run="title"` | `web-run.js` asserts it on every run's title; it failed on the build before ("the title didn't mark the page") and passes after | pass | verified (browser) |
| 2 | Marking the title doesn't count as a run | the `title-click` run still sees no `data-runs` after a click, and every run's wait on `data-runs` still starts it | pass | verified (browser) |
| 3 | A lap counts in `data-laps`, not `data-runs`; a run resets `data-laps` | `PlayerTrackMovement.LapSequence` calls `ResetRun(lap: true)`; a death, restart or way out calls `ResetRun()`; the jslib sets `data-laps` to 0 on a run | pass | by reading; no browser run reaches a lap yet |
| 4 | The build runs with it | `scripts/check-web.sh --build`: every run, including `title-click` | pass | verified |

Also: DIRECTION.md's stale lines (the C key, "no toasts") and the mirror hints' hardcoded SHIFT.

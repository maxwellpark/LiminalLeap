# Acceptance: max/title-click

A click on the title only focuses the game; a key starts a run.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A click on the title doesn't start a run | `web-run.js --only=title-click`: clicks the canvas, waits up to 5 s for any run to mark the page | pass | verified (browser) |
| 2 | The check catches the old behaviour | the same check on the build before the fix: "a click on the title started a run", exit 1 | fail as expected | verified (browser) |
| 3 | After the click, Enter still starts today's run | the same scenario then presses Enter and reads `data-run` daily, `data-place` office | pass | verified (browser) |
| 4 | The other runs still start | `scripts/check-web.sh --build`: story and every place's daily | pass | verified |

## Unverified

- Inside itch's frame: the click is the same `pointerdown` on the canvas, but the page isn't up.
- A tap on a phone no longer starts the story; the page is marked not mobile friendly.

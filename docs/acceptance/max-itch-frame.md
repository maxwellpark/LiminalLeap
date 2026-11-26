# Acceptance: max/itch-frame

The browser check plays the game as itch embeds it.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Space after the page's launch button reaches the game in a cross-origin iframe, and the page doesn't scroll | `web-run.js --only=itch-frame`: the parent on `localhost`, the game on `127.0.0.1`, no focus call from the check | pass | verified (browser) |
| 2 | The check catches a page that doesn't take focus | the same run on a copy of the build with the template's `canvas.focus()` removed: "Space after the launch button didn't reach the game", "the page scrolled 700px under the game" | fail as expected | verified (browser) |
| 3 | The other runs still pass | every `web-run.js` run against the current build | pass | verified (browser) |

## Unverified

- itch's own page: its launch button, iframe attributes and any focus handling of its own.

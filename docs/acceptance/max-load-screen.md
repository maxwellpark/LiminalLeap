# Acceptance: max/load-screen

The load screen names the game plainly, and a failed load shows a sentence a player can act on
in place of the loading screen, not over it.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The tab and the loading text say "Liminal Leap" | the built `Build/WebGL/index.html` has `<title>Liminal Leap</title>` | pass | verified (build) |
| 2 | A browser without WebGL 2 gets a sentence, and Unity never loads | `web-run.js --only=no-webgl2` stubs `getContext("webgl2")` to null: the error starts "This browser can't run the game", the loading screen is hidden, and no `Build/` file is requested | pass | verified (browser) |
| 3 | The check catches the old page | the same run on the build before: "the loading screen is still up under the error"; the capture shows Unity's message drawn over "LIMINALLEAP" | fail as expected | verified (browser) |
| 4 | A probe that throws still loads the game | the probe is in a try/catch that leaves `noWebGL2` false | pass | by reading |
| 5 | Errors can't inject markup, and long ones wrap | `textContent`, `white-space: pre-line`, `overflow-wrap: anywhere` | pass | by reading |
| 6 | The game still loads and runs | `scripts/check-web.sh --build`: every run | pass | verified |

## Unverified

- A real browser without WebGL 2, and Unity's own load failures, which the check can't cause.

# Acceptance: max/title-options

The title told a player to press P then F to turn the flashing down, but on the title P starts
the story, which flickers from its first second. F and M now work on the title itself.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | F on the title turns reduced flashing on and starts nothing | `web-run.js --only=title-options`: after F the page still says `data-run="title"` with no run counted, and `data-reduced-flashing` is 1 | pass | verified (browser) |
| 2 | The choice is kept for the next visit | the same run reloads the page: still 1 | pass | verified (browser) |
| 3 | The check catches the old title | on the build before: "F on the title started story" | fail as expected | verified (browser) |
| 4 | The title shows both options and their state | `ControlsTests.TheTitleWarnsOfFlashingAndSaysHowToTurnItDown`; `Build/captures/title-options.png`: "F  reduced flashing  on     M  sound  on" | pass | verified (.NET, browser) |
| 5 | Every other run still passes | `scripts/check-web.sh --build`: thirteen runs | pass | verified |

## Unverified

- M on the title by ear.

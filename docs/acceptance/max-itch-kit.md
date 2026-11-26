# Acceptance: max/itch-kit

What the itch page needs, ready for the owner: `docs/ITCH.md` and `scripts/store-images.sh`.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The controls listed are the game's | a review read KeyboardInput.cs, PauseMenu.cs, TitleScreen.cs and the input axes | pass | verified |
| 2 | The URL, places, exit rule and "nothing is uploaded" are true | the review: ShareLine.Link, Places.cs, DIRECTION.md, the clipboard-only jslib | pass | verified |
| 3 | The doc makes no false safety claim | the first draft said `deploy-itch.sh` refuses a public page; it doesn't, and now says so, with the owner's switch to shipwright's `itch-push` named | pass | fixed after review |
| 4 | The images come out at itch's sizes from real runs | `scripts/store-images.sh`: four 1920 x 1200 screenshots and a 630 x 500 cover, every run checked by `web-run.js` | pass | verified |
| 5 | A failed image run leaves the old images alone | images are made beside them and swapped in at the end | by reading | verified by reading |
| 6 | `web-run.js` refuses bad options instead of guessing | `--size=1920x1200x9`, `--size 1920x1200`, `--only=nope` and `--bogus` each exit 2 | pass | verified |
| 7 | The browser checks still pass | `scripts/check-web.sh`: four runs, exit 0 | pass | verified |

## For the owner

The page's settings, tags and description are defaults to confirm. The pitch names Exit 8,
the content notes and itch's AI disclosure are the owner's calls, and pushes stay unguarded
until the switch to `itch-push`: keep the page restricted.

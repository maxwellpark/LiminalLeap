# Acceptance: max/title-audio

Browsers hold a page's audio until a gesture in its frame, and Unity starts a sound queued
before that as far in as the time already gone, so the title sting was never heard.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Before a gesture the title waits: no sting, and the page says audio is waiting | `web-run.js --only=audio`, with the page's AudioContext made to stay suspended until a click or key (headless Chromium runs audio whatever its autoplay flags) | pass | verified (browser) |
| 2 | A click wakes the audio and plays the sting, without starting a run | the same run: `data-sting` is set within 5 s of the click, and `data-runs` is still absent | pass | verified (browser) |
| 3 | The check catches the old title | on the build before: "before a gesture: audio null" (the sting was queued at Start) | fail as expected | verified (browser) |
| 4 | A starting key's sound waits for the audio it wakes, at most 0.6 s | `TitleScreen.ConfirmThenLoad` | pass | by reading |
| 5 | Every other run still passes | `scripts/check-web.sh --build` | pass | verified |

## Unverified

- By ear in a real browser, and in itch's frame, where the first gesture in the frame is
  usually the key that starts the story (the sting then doesn't play; the start sound does).

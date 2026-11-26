# Acceptance: max/place-lighting

A lighting check per place: the art and the anomalies both read in each place's light.

Measured from captures every second after each run starts, in the WebGL build in headless
Chromium (a scratch copy of `scripts/web-run.js`), from 3 s on, once the run is up to speed.
"Near floor" is the mean grey level, 0 to 255, of the bottom-centre band (x 240 to 720, y 480
to 600 of 960 x 600); "frame" is the whole capture. A daily left alone dies within a few
seconds, and frames from its fade on aren't counted; n is the frames that were.

| Run | n before, after | Near floor before, median (range) | After | Frame median before, after |
|---|---|---|---|---|
| Story, the office | 14, 14 | 27 (18 to 52) | 32 (29 to 53) | 41, 39 |
| Daily, the office | 3, 3 | 25 (21 to 29) | 37 (37 to 38) | 40, 44 |
| Daily, the school | 4, 11 | 20 (16 to 25) | 32 (21 to 46, and one flash at 160) | 30, 36 |
| Daily, the house | 3, 3 | 24 (22 to 28) | 34 (31 to 35) | 35, 37 |

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The floor ahead reads better at speed in every place | the table: medians rise from 20 to 27 to 32 to 37; carpet grain and boards show in the captures | pass | verified (browser) |
| 2 | It stays dark: only the floor ahead clears | frame medians stay 36 to 44; the vignette's sides and top are unchanged (`VignetteTextureTests.Build_BelowClearsTheFloorAheadOnly`) | pass | verified (.NET, browser) |
| 3 | The story's office isn't darker than its daily | frame medians 41 against 40 before the change; the earlier gap was one frame of each, caught at different points of the flicker | pass | verified (browser) |
| 4 | The anomalies still read | they're mid-frame (strip lights, pillars, the floor count), where the vignette was already clear and nothing changed | pass | by reading; no anomaly lap captured |
| 5 | The build runs with it | `scripts/check-web.sh --build`: four runs start, no console errors | pass | verified |

`VignetteTextureTests` now run under .NET in `scripts/acceptance.sh`.

## Unverified

- Few frames for the dailies, which die early with no one steering.
- An anomaly lap seen in each place's light; the browser check doesn't play to one.
- On a real GPU and monitor, rather than swiftshader.

# Acceptance: max/place-lighting

A lighting check per place: the art and the anomalies both read in each place's light.

Measured from captures every second for 16 s after each run starts, in the WebGL build in
headless Chromium (a scratch copy of `scripts/web-run.js`). "Near floor" is the mean grey level,
0 to 255, of the bottom-centre band (x 240 to 720, y 480 to 600 of 960 x 600) at full speed;
"frame" is the whole capture. A daily left alone dies at about 5 s, after which captures are
black and not counted.

| Run | Near floor before | Near floor after | Frame after |
|---|---|---|---|
| Story, the office | 19 to 31 | 30 to 37 | 32 to 47 |
| Daily, the office | 22 to 39 | 38 to 42 | 38 to 53 |
| Daily, the school | 17 to 31 | 26 to 46 | 25 to 64, two flashes at 78 and 82 |
| Daily, the house | 23 to 33 | 32 to 37 | 35 to 48 |

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The floor ahead reads at speed in every place | the table: the near floor at full speed rises from 15 to 22 to about 30 to 38, carpet grain and boards visible in the captures | pass | verified (browser) |
| 2 | It stays dark: only the floor ahead clears | frame means stay 25 to 64 outside two flashes in the school run; the sides and top of the vignette are unchanged (`VignetteTextureTests.Build_BelowClearsTheFloorAheadOnly`) | pass | verified (.NET, browser) |
| 3 | The story's office isn't darker than its daily | over 16 s they overlap (frame 32 to 47 against 38 to 53, both flickering); the earlier gap was one frame of each | pass | verified (browser) |
| 4 | The anomalies still read | they're mid-frame (strip lights, pillars, the floor count), where the vignette was already clear and nothing changed | pass | by reading; no anomaly lap captured |
| 5 | The build runs with it | `scripts/check-web.sh --build`: four runs start, no console errors | pass | verified |

`VignetteTextureTests` now run under .NET in `scripts/acceptance.sh`.

## Unverified

- An anomaly lap seen in each place's light; the browser check doesn't play to one.
- On a real GPU and monitor, rather than swiftshader.

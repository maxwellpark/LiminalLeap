# Acceptance: max/pixel-ratio

The canvas renders at no more than 1.5 device pixels per CSS pixel.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | On a 2x screen the canvas renders at 1.5x its CSS size | `web-run.js --only=hidpi`, Chromium at device scale 2: `canvas.width / clientWidth` between 1.45 and 1.51, no console errors | pass | verified (browser) |
| 2 | The check catches an uncapped page | the same run on the build before the cap: "renders at 2.00x its size on a 2x screen" | fail as expected | verified (browser) |
| 3 | Unity reads the cap on resize and its own fullscreen, not only at load | Unity 6's `_JS_SystemInfo_GetPreferredDevicePixelRatio` returns `Module.devicePixelRatio` first, and is read on each resize | pass | by reading the built framework |
| 4 | The other runs still pass | `scripts/check-web.sh --build`: every run | pass | verified |

## Unverified

- A window moved from a 1x screen to a 2x one keeps the ratio it loaded with (1); a reload picks up the cap.
- Frame time on a real high-DPI laptop; swiftshader can't say.

# Acceptance: max/title-controls

The title's controls name restart, the other mirror key and the reduced flashing option, and say
the lights flicker.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The title names every key a run uses, beside its action | `ControlsTests.TheTitleNamesEveryKeyARunUses`: "A D  steer" through "P  pause"; that each matches the `KeyCode` the input reads is by reading (`KeyboardInput`, `PauseMenu`), since `KeyCode` isn't available under .NET | pass | verified (.NET), keys by reading |
| 2 | It warns of flashing and says how to turn it down | `ControlsTests.TheTitleWarnsOfFlashingAndSaysHowToTurnItDown` | pass | verified (.NET) |
| 3 | No two actions share a key | `ControlsTests.EveryKeyIsDistinct` | pass | verified (.NET) |
| 4 | It fits and reads on the title | `Build/captures/story-title.png` from `scripts/check-web.sh --build`: both lines on one screen at 960 x 600 | pass | verified (browser) |
| 5 | Right Shift looks back as the title says "SHIFT" | `KeyboardInput.LookingBack` reads both Shift keys and Q | pass | by reading |
| 6 | The pause screen's labels come from the same place | `PauseMenu.Refresh` labels with `Controls.ToTitle`, `ReducedFlashing`, `Mute`; its input still reads `KeyCode.T`, `F`, `M`, so a rebind changes both | pass | by reading |

`scripts/acceptance.sh`: 134 tests pass; `Controls.cs` now builds under .NET.

## Unverified

- Right Shift in a browser; the check can't hold a key through a run yet.

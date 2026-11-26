# Acceptance: max/footsteps

Footsteps that sound like each floor, timed to the running bob.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A step lands where the bob is deepest | `FloorsTests.LowPointsAreWhereTheBobIsDeepest`: -\|sin\| is -1 at each low point, and crossings are counted there | pass | verified (.NET) |
| 2 | At running speed steps keep a runner's cadence, each on a low point | `FloorsTests.AtSpeedStepsKeepARunnersCadence`: 8, 15 and 22 m/s at 60 fps give 4.0 to 4.1 steps a second | pass | verified (.NET) |
| 3 | Slow enough, every low point is a step | `FloorsTests.SlowEnoughEveryLowPointIsAStep` | pass | verified (.NET) |
| 4 | No step standing still, or on the landing's thud | `FloorsTests.StandingStillTakesNoSteps`, `ALandingHoldsOffTheNextStep` | pass | verified (.NET) |
| 5 | The step sound is short and sane | `SynthTests.EverySound_ProducesASaneBuffer` and the one-shot length test now include `Sound.Step` | not run | Unity EditMode, needs a licence |
| 6 | No step while airborne, dying or paused | read: steps are only reached from `HandleBob`'s grounded branch, after `Update` returns early when dying or paused | traced | unverified by test |
| 7 | The build runs with it | `scripts/check-web.sh --build`: four runs start, no console errors | pass | verified |

`scripts/acceptance.sh`: 120 tests pass.

## By ear

The browser check runs muted, so how they sound is for a person: start the story (carpet), then
a daily on tile or boards, and listen at full speed. They should read as running, quieter than
a landing, softer on carpet and brighter on tile.

## Unverified

- The sound itself, by ear.
- The Unity suites (no licence here); the builds came from an unlicensed editor, which max-kit
  allows for development checks.

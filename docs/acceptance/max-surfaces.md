# Acceptance: max/surfaces

Surfaces (#56): four floors, each changing how you move as well as how the corridor looks.
Arcade runs change floor in stretches; each story place keeps one floor.

To reproduce every number here, run `LIZARD=<path to lizard> scripts/acceptance.sh`. It needs
.NET 8; lizard is optional and only prints complexity. No Unity suite has run, because there's
still no licence on this machine. Anything marked "not run" is written but unproven.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Linoleum moves exactly as before | `FloorsTests.LinoleumMovesAsTheGameWasTuned` | pass | verified (rule) |
| 2 | Wet tile slides, carpet softens the bob and muffles a landing, boards bob deeper and knock lower | `EachOtherFloorChangesHowYouMove`, transcript | pass | verified (rule); feel unverified |
| 3 | An arcade run opens on linoleum | `ARunOpensOnLinoleum` (4 seeds), `FloorTests.AFreeRunChangesFloorInStretches` (not run) | pass / not run | verified (rule) |
| 4 | Floors change every 8 to 14 pieces, never to the same floor | `StretchesRunEightToFourteenPieces` (2,000 pieces), transcript | pass | verified (rule) |
| 5 | All four floors turn up | `EveryFloorTurnsUp` | pass | verified (rule) |
| 6 | The same seed lays the same floors, so a daily is fair; another seed lays others | `TheSameSeedLaysTheSameFloors`, `AnotherSeedLaysOthers`, `AskingBackwardsGivesTheSameAnswer` | pass | verified (rule) |
| 7 | A story place keeps one floor: office carpet, school linoleum, house boards | `FloorTests.APlaceKeepsItsOwnFloor` (not run); `Places.Story` | not run | unverified |
| 8 | The daily varies like a free run | the generator only takes a place's floor in the story (`RunMode.Story`), as the review traced | by reading | verified by reading |
| 9 | A looped stretch keeps its floors | `LoopTests.TheLoopingDoorLaysTheTrackAlreadyRun` floor check (not run); memory stores the floor | not run | unverified |
| 10 | The `Surfaces` flag off means all linoleum | `FloorTests.WithTheFlagOffItIsAllLinoleum` (not run) | not run | unverified |
| 11 | Floorboard seams run down the length of the run | `ProceduralTexturesTests.StripesRunOneWay` | pass | verified |
| 12 | A pooled piece goes back to the baked linoleum when reused | `TrackPiece.SetFloor` keeps the prefab's material from the first call | by reading | verified by reading |
| 13 | No frame drop the first time a new floor appears | the three floor materials are built at run start | by reading | unverified in a build |
| 14 | Every assembly compiles in Unity 6000.2.15f1 | batch compile, no errors | pass | verified |

## Paths

| Run | Floors | Evidence |
|---|---|---|
| free or daily, flag on | stretches from the run's seed, opening on linoleum | claims 3 to 6 |
| story place, flag on | the place's floor throughout | claim 7 |
| any run, flag off | linoleum throughout | claim 10 |
| through a looping door | the remembered pieces' floors again | claim 9 |
| run reset | footing back to linoleum until the first piece is read | by reading |

## Numbers

- Tests outside Unity: 67 passed, 0 failed. 21 are new here: 10 for floors and the 11 texture
  tests (one new), which now run outside Unity too.
- Coverage of the pure rules: line 100%, branch 98.4% overall. `FloorRule` and `FloorFeel` are
  100% / 100%. The gap is in the older `ProceduralTextures` code, measured here for the first time.
- Complexity: `FloorRule.At` is 4, `TrackPiece.SetFloor` 6 and `Stripes` 4. `ResetRun` rose
  from 11 to 14 and `SpawnNext` from 28 to 29. Splitting `SpawnNext` is the next Ready item.

## Transcript

From `scripts/acceptance.sh`, the real `FloorRule` and `FloorFeel`:

```
== Floors in a free run: stretches of 8 to 14 pieces, seed 1234
   pieces   0-  8: Linoleum grip x1.00  bob x1.00  landing pitch x1.00, volume x1.00
   pieces   9- 16: Boards   grip x1.00  bob x1.35  landing pitch x0.70, volume x1.10
   pieces  17- 25: Tile     grip x0.35  bob x1.00  landing pitch x1.15, volume x1.00
   pieces  26- 35: Linoleum grip x1.00  bob x1.00  landing pitch x1.00, volume x1.00
   pieces  36- 45: Tile     grip x0.35  bob x1.00  landing pitch x1.15, volume x1.00
   pieces  46- 59: Linoleum grip x1.00  bob x1.00  landing pitch x1.00, volume x1.00
   pieces  60- 68: Tile     grip x0.35  bob x1.00  landing pitch x1.15, volume x1.00
   pieces  69- 76: Carpet   grip x1.25  bob x0.60  landing pitch x0.80, volume x0.55
   pieces  77- 90: Tile     grip x0.35  bob x1.00  landing pitch x1.15, volume x1.00
   pieces  91-101: Carpet   grip x1.25  bob x0.60  landing pitch x0.80, volume x0.55
   pieces 102-114: Linoleum grip x1.00  bob x1.00  landing pitch x1.00, volume x1.00
   pieces 115-119: Boards   grip x1.00  bob x1.35  landing pitch x0.70, volume x1.10
```

## By hand

1. Start a free run. The first stretch is the familiar linoleum.
2. After 80 to 140 m the floor ahead changes, and the seam is visible before you reach it.
3. On wet tile (pale, glossy small tiles), tap a strafe: you drift into the lane and drift past
   it before stopping. On carpet the same tap is crisp.
4. Jump on each floor. The landing should thud on carpet, knock on boards and click on tile.
5. Watch the camera bob: flatter on carpet, deeper on boards.
6. In a free run, take a looping door: the replayed stretch has the same floors.
7. Story: the office is carpet throughout, the school linoleum, the house boards.
8. Turn the `Surfaces` flag off in the debug overlay and restart: linoleum everywhere.

## Unverified

- Every Unity suite, including the three FloorTests and the LoopTests floor check.
- The feel numbers: they're first guesses, recorded as a default for review in
  `docs/DIRECTION.md`. Wet tile at 0.35 grip may be too slippery at full speed.
- The look of the three new floors: built from code, never seen rendered.
- The landing sound's pitch on each floor, through the procedural library.

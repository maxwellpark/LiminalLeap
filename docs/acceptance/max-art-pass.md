# Acceptance: max/art-pass (#102)

The groundwork for generated art: the pipeline doc, a 25 MB budget on the web build, and pieces
dressed with a place's generated floors, walls, ceiling and props, keeping the procedural look
where there are none.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | With no art files, the game looks and plays as before | the final build of this branch run in a browser: the office as before, no console errors; review traced no new per-spawn garbage | pass | verified |
| 2 | A floor kind's texture replaces it wherever it's laid | placeholder `Carpet.png`: the office floor showed it | pass | verified (browser) |
| 3 | A place's wall and ceiling textures add walls behind the pillars and a ceiling above the lights | placeholder `Wall.png` and `Ceiling.png` in the office run | pass | verified (browser) |
| 4 | About a third of pieces stand one prop, chosen by seed, beside the track | placeholder box prop in the office run; the hash gives 22 of 60 test seeds and about 35% over 100,000 (review) | pass | verified (browser, hash) |
| 5 | Walls, ceiling and props can't be hit | colliders removed on creation; `PlaceArtTests.WallsAndCeilingWearTheirArtAndCanNotBeHit` (EditMode) | not run | verified by reading |
| 6 | A pooled piece laid in a place without art loses the last one's | `ArtGoesWhenTheNextLayingHasNone` (EditMode) | not run | verified by reading |
| 7 | A release build over 25 MB fails and lists the largest assets | the budget logic against the real build: passes at 25 MB, fails at 5 MB with the list | pass | verified |
| 8 | Every assembly compiles in Unity 6000.2.15f1 | batch compile, no errors | pass | verified |

## Numbers

- WebGL build: 10 MB of the 25 MB budget, with or without the placeholder art.
- Tests outside Unity: 115 pass. The five new EditMode tests need the Unity runner.

## By hand

1. Drop a 512 px PNG at `Assets/Resources/Art/Floors/Carpet.png`, build, and start the office:
   the floor wears it.
2. Add `Assets/Resources/Art/Places/office/Wall.png` and `Ceiling.png`: walls and a ceiling
   appear in the office and in a daily wearing its look, never in a free run.
3. Remove them: the office is back to the open corridor.

## Unverified

- The EditMode tests in `PlaceArtTests`, which need the Unity runner.
- Turns: a wall is one straight section per piece, so a turning piece leaves a dark gap along
  its outside (in the backlog).
- The builds came from an unlicensed Unity editor, which max-kit allows for development checks.

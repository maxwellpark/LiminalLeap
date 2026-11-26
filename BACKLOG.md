# Backlog

The queue for GOAL.md. Ready is worked top down; each item names the Done when line it moves.

## Ready

Map the space (#61), in slices, each its own PR:

1. A place's laps are one fixed corridor: the same pieces, scenery, calm stretches and floor
   every lap and every run, seeded from the place, with anomalies still drawn per lap. Changes
   how the story plays, so it lands behind a flag first.
2. Detour forks in a place's lap, clear of the anomaly stretch: the looping door replays the
   last stretch without counting it toward the lap.
3. The map's record per place in the save: forks reached and doors taken, anomalies seen,
   floors and landmarks along the lap. The record itself is built (`PlaceMap`, save v5); what
   remains is marking it during a run, which needs slices 1 and 2 to fix where things are.
4. The map drawn between runs, on the run summary and the place's title card.
5. The hidden door and second way out, once its content is designed (owner).

Slices 1 and 2 change the generator in ways only the PlayMode suites can check, so they're
safer once the Unity licence is set up.

Art pass, the office first (decided 2026-11-25), each its own PR:

1. The office's 3 to 5 props, generated and committed by following `docs/ART.md`. On the
   owner's machine, since image-to-3D needs a GPU. Its textures are done.
2. The props as map landmarks, once the map is marked during runs.

## Blocked on owner

- Unity licence on a cloud machine: the automation Unity ID, the LiminalLeap 1Password vault
  and service account, `OP_SERVICE_ACCOUNT_TOKEN` in the environment, and the 1Password
  sign-in address (docs/SETUP.md). Blocks every Unity suite, so "Every Unity suite passes" and
  "Each story place can be got out of".
- Shared tools: switch `publish-itch.sh` and `deploy-itch.sh` to shipwright's `itch-push`, and
  choose (a) extend unity-packages' `unity-verify` for Linux or (b) a recorded exception for
  `check.sh`'s test runner. `publish-itch.sh` likely misses butler's login on macOS meanwhile.
- The itch page (#74), its account steps and the first push. Blocks "A WebGL build is on itch".
- Outside players for a story place. Blocks "At least 5 people outside the project".
- A blocked-terms file for this repo: max-kit's shared hooks refuse pushes without one, so each
  push from here so far has been made with the hooks taken off, on an explicit ask.

## Ideas

- A place's best counts only the last lap's score, since each lap resets it. Hidden in the
  story today; matters if place bests are ever shown.
- Forks in the story places, which need a design that fits the exit rule.
- Vertical and split pursuer attacks (#67), an attack director (#68), distorted telegraphs
  (#69), attacks per place (#70).
- Statics out of the run state (#22), the new Input System (#7).

## Done

- 2026-11-25: walls, ceiling and side floor reach 1.1 m past a turning piece's end, across
  the wedge a 7 degree turn opens at the walls (`max/wall-turns`).
- 2026-11-25: the office's carpet, wall and ceiling textures, generated on a CPU with Stable
  Diffusion 1.5 (sources in `Assets/Art/Sources.md`) and finished by
  `tools/art/tiles.py`. Textures repeat every 2 m, and a place with walls gets floor out to
  them (`max/office-textures`).
- 2026-11-25: the art pass's groundwork (`max/art-pass`): `docs/ART.md`; generated floors,
  walls, a ceiling and props load by path and fall back to the procedural look; the web build
  fails past 25 MB. Checked in a browser with placeholder art in the office, then removed.
- 2026-11-25: the GitHub workflows are gone, since Actions is off and they never ran; #92's
  deploy workflow closed unmerged, as `scripts/deploy-itch.sh` deploys from a machine. The web
  build now generates its scenes first, which it needed: `scripts/build-web.sh` built a 10 MB
  WebGL build here (`max/no-actions`).
- 2026-11-25: the map's record (`PlaceMap`), one per place in the save, which moves to v5. The
  save code and its tests now run outside Unity too (`max/place-maps`).
- 2026-11-25: the looping door's tell: on the fork piece, the pillars on the looping side stand
  as they did where that door's replay begins (`max/door-tell`). Its LoopTests check is written
  but not run.
- 2026-11-25: `build-mac.sh` takes Unity's path from `scripts/unity-path.sh` and refuses only
  while this project is open, as the web build does. Not run: it needs a Mac.
- 2026-11-25: `SpawnNext` split into choosing, taking, readying, placing and remembering a
  piece (complexity 31 down to 8 at most), as a pure refactor checked by review rather than the
  Unity suites, which still can't run here.
- 2026-11-25: #95 and #96 merged, after a review that fixed an exit shifting the replayed
  distances, the fork wall taking its piece's floor, and the floors being baked as assets.
- 2026-11-25: surfaces (#56): linoleum, carpet, wet tile and floorboards, each with its own
  look, grip, bob and landing sound. Arcade runs change floor every 8 to 14 pieces from the
  run's seed; each story place keeps one (`max/surfaces`). FloorTests and the LoopTests floor
  check are written but not run.
- 2026-11-25: the distance that lies (#54): through a looping door the HUD's distance drops
  back to what it read at the start of the replayed stretch and stays that far behind; the score,
  summary and share line keep the true distance (`max/distance-that-lies`). Its LoopTests are
  written but not run.
- 2026-11-25: the acceptance projects are tracked; Unity's `*.csproj` rule had kept them out, so
  `scripts/acceptance.sh` failed from a fresh clone.
- 2026-11-25: #94 merged, with #91 under it.
- 2026-11-24: audit pass (first-session player, robustness, hot paths, tests). Landed in
  ba64c0a: laps kept their length through breaths, laps start afresh each run, story laps out
  of the arcade ghost, the floor count held still, a lap test's missing frame. Not landed: a
  place's best only sees its last lap's score, which the story never shows (Ideas).
- 2026-11-24: the page marks itself ready (`data-ready="1"`) once the title is up, for
  `web-check --ready-marker` on a WebGL build.
- 2026-11-24: forks in the arcade track, one door laying the track already run again (db92197).
  Its PlayMode tests (LoopTests) are written but not run, so the goal line stays open.
- 2026-11-24: the loop rule and a memory of the track already laid (a6b748e).
- 2026-11-24: anomalies on the exit rule, the share line with its button and story card, the
  daily taking a place's look only (#93).

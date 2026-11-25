# Backlog

The queue for GOAL.md. Ready is worked top down; each item names the Done when line it moves.

## Ready

Nothing buildable is left for an unmet goal line: the rest wait on the owner, below.

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
- A tell for which door loops, if a blind choice plays as a coin flip.
- Map the space (#61), once loops exist.
- Vertical and split pursuer attacks (#67), an attack director (#68), distorted telegraphs
  (#69), attacks per place (#70).
- Statics out of the run state (#22), the new Input System (#7).
- The dev scene out of shipped builds (#76).
- `build-mac.sh` sourcing `scripts/unity-path.sh`.

## Done

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

# Backlog

The queue for GOAL.md. Ready is worked top down; each item names the Done when line it moves.

## Ready

1. Loops (#51), the rule: where a fork goes, which door loops, and a buffer of the track
   already run that a looping door replays piece for piece. Pure, with tests. Moves: "A place
   loops back on itself".
2. Loops (#51), in the generator: a fork piece with a wall and two doors in the arcade runs,
   spawning held at the fork until the player commits, then either fresh track or the replayed
   stretch. Behind a flag, with PlayMode tests. Moves: "A place loops back on itself".

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
- Pushing the branch: commits from this session wait on the owner's word.

## Ideas

- Distance that lies (#54), once loops exist.
- Map the space (#61), once loops exist.
- Vertical and split pursuer attacks (#67), an attack director (#68), distorted telegraphs
  (#69), attacks per place (#70).
- Surfaces that behave differently (#56).
- Statics out of the run state (#22), the new Input System (#7).
- The dev scene out of shipped builds (#76).
- `build-mac.sh` sourcing `scripts/unity-path.sh`.
- A ready marker in the web template, so shipwright's `web-check` can tell a started game
  from a stuck loading bar.

## Done

- 2026-11-24: anomalies on the exit rule, the share line with its button and story card, the
  daily taking a place's look only (#93).

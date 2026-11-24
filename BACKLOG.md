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
- Pushing the branch: commits from this session wait on the owner's word.

## Ideas

- Forks in the story places, which need a design that fits the exit rule.
- A tell for which door loops, if a blind choice plays as a coin flip.
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

- 2026-11-24: forks in the arcade track, one door laying the track already run again (db92197).
  Its PlayMode tests (LoopTests) are written but not run, so the goal line stays open.
- 2026-11-24: the loop rule and a memory of the track already laid (a6b748e).
- 2026-11-24: anomalies on the exit rule, the share line with its button and story card, the
  daily taking a place's look only (#93).

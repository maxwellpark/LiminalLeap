# Log

## 2026-11-24

Backlog session started at 83cb4be, working GOAL.md's one unmet line that can be built here: loops.
Unity compiles here without a licence and editor methods run, but tests are refused without
one (exit 198). They aren't run through an editor method instead, since that would get round
Unity's licence check; Unity runs here are compile checks only.

- Loops, the rule (a6b748e): forks due every 24 pieces after the first 20, the looping door
  picked from the run's seed, and a ring buffer of recent pieces. 43 pure tests pass under .NET.
  Default 2026-11-24 [needs-review]: forks in the arcade runs only, every 24 pieces, replaying the
  last 10. Why: the story's exits are already calls, and a fork inside a lap would fight the
  exit rule. To change: the numbers are serialised on the generator; story forks need a design.
- Loops, in the generator (db92197): forks in the arcade runs behind the `Loops` flag, spawning
  held at the fork until the player passes the wall, the looping door replaying the remembered
  stretch. A review found a replay could put a hazard in a breath and drift on repeat loops;
  fixed before the commit. Compiled in Unity; LoopTests not run.

End of session. Landed: the review items (daily look only, the event strip, the Copy button and
preview, the story card, the max-kit licence exception) and loops. Half done: nothing; every
change is committed. Defaults taken: loops in the arcade only, with their numbers
(docs/DIRECTION.md). Blocked on the owner: the Unity licence and 1Password setup, the shared
tools choice, the itch page, players, and pushing this branch. Next: run the suites once there's
a licence, then tick the loops line; or a design for forks in the story.

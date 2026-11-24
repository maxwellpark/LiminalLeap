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

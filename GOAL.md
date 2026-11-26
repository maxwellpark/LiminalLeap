# Goal

Status: confirmed 2026-11-24. Outcome, Done when and Out of scope change only on the owner's
word. Detail and reasoning live in docs/DIRECTION.md.

## Outcome

A short liminal horror runner on itch: three story places where the only warning is in your
mirror and the exit rule decides whether you get out, plus a daily anyone can share. For
players of short horror, and the streamers who play it.

## Done when

- [ ] Every Unity suite passes: `scripts/check.sh` exits 0.
- [ ] Each story place can be got out of, start to end: `StoryTests` and `LapTests` pass.
- [ ] A place loops back on itself (#51): a junction returns to track already run, with a test.
  Built and compiled in db92197 (LoopTests); open until the test has run.
- [ ] A WebGL build is on itch: `scripts/deploy-itch.sh` pushed, and a run starts in the browser.
  The browser half holds: `scripts/check-web.sh --build` starts every run (2026-11-26).
- [ ] At least 5 people outside the project have finished a story place.
  How to run the sessions and what counts: docs/PLAYTEST.md.

## Numbers

- Unity tests passing, from `scripts/check.sh`: not run yet, no licence on the machines
  used so far.
- Run loop allocations over baseline, from `AllocationTests`: 577 B/frame.
- WebGL build size, from `scripts/build-web.sh`: 11 MB of a 25 MB budget.
- Pure tests under .NET, from `scripts/acceptance.sh`: 136 passing.

## Out of scope

- Power upgrades: progress is depth and knowledge of the place, never strength (#60).
- Leaderboards and accounts: the daily stays local, shared by pasting a line.
- A Backrooms skin: liminal places, not someone else's look and lore.
- Platforms beyond the browser until itch players have played it.

## Last checked

2026-11-26, at 17b5a41, with the WebGL build checked in a browser (nine runs, all passing).

# Acceptance: max/door-tell

The looping door's tell. On the fork piece, the pillars on the looping door's side stand as
they did on the first piece of the stretch that door lays again: the same pillars are up, at
the same heights. The other side and the lights are untouched. It is meant to be faint: an
attentive player can notice that one side looks like a while ago.

This branch is stacked on #97 (the `SpawnNext` split). No Unity suite has run, because there's
still no licence on this machine.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The tell copies the first replayed piece's pillars on the looping side only | `TrackSceneryTests.AnEchoCopiesOneSideAndLeavesTheOther` (EditMode) | not run | unverified |
| 2 | A pooled fork piece reused as ordinary track loses the tell | `TrackSceneryTests.TheNextVaryClearsAnEcho` (EditMode) | not run | unverified |
| 3 | At a real fork, the looping side matches where the replay begins | `LoopTests.TheLoopingSideStandsAsTheReplayBegins` (PlayMode) | not run | unverified |
| 4 | The seed copied is the one the replay's first piece will be laid with | `LoopRuleTests.TheOldestOfTheLastFewIsWhereAReplayBegins`, `AnEmptyMemoryHasNoOldest`; review traced `TryOldestOfLast` against `CopyLast` | pass | verified (rule); generator by trace |
| 5 | The side is the looping door's: left is local -x, as `LoopRule.DoorFor` reads it | review traced `MakeFork` and `TryCommit` | by reading | verified by reading |
| 6 | Splitting out `VaryPillar` doesn't change any piece's scenery | review compared it with the old loop, hash salts included | by reading | verified by reading |
| 7 | The test seeds really differ, so the EditMode test can fail | the scenery hash computed for seeds 11 and 500: three of four left pillars differ | pass | verified |
| 8 | Every assembly compiles in Unity 6000.2.15f1 | batch compile, no errors | pass | verified |

## Numbers

- Tests outside Unity: 76 passed, 0 failed (2 new). `TrackMemory` coverage 100% line and branch.
- Complexity: `Echo` 3, `VaryPillar` 3, `EchoReplay` 3, `TryOldestOfLast` 2.

## By hand

1. Start a free run. At about 200 m the first fork comes: a wall with a door either side.
2. Before it, note the pillars about 100 m back (ten pieces).
3. At the fork, one side's pillars match that stretch. Take that door: the track you get is the
   stretch you noted. Take the other door next time: fresh track.
4. Whether it reads as a tell or just variation is the question this ships to answer.

## Unverified

- Every Unity suite, including the three tests above.
- How readable the tell is at speed: pillars are 2.5 m apart and the lane is 6.5 m out.
- With the `HangingPillars` anomaly on a fork piece, the anomaly would hide the tell. Anomalies
  only run in story places, which have no forks, so this can't happen today.

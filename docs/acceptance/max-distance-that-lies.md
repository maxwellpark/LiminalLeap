# Acceptance: max/distance-that-lies

The distance that lies (#54). It only lies in arcade runs: after a looping door, the HUD's
distance drops back to what it read at the start of the stretch being run again, and stays
that far behind for the rest of the run. Everything that counts keeps the true distance.

Reproduce every number here with `LIZARD=<path to lizard> scripts/acceptance.sh` (needs .NET 8;
lizard is optional and only prints complexity). No Unity suite has run: there's still no
licence on this machine, so anything marked "not run" is written but unproven.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Without a loop the HUD tells the truth | `DistanceReadoutTests.WithoutALoopItTellsTheTruth`, transcript | pass | verified (rule) |
| 2 | Through a looping door the HUD rewinds to the reading at the start of the replayed stretch, then counts on in step | `ThroughALoopTheNumbersRepeat`, transcript | pass | verified (rule) |
| 3 | The rewind waits until the replay starts, so the door's own piece still counts on | `ItCountsOnUntilTheReplayStarts`, transcript (209 m then 90 m) | pass | verified (rule) |
| 4 | Each replayed piece reads what it read the first time, even where an exit was left out of the replay | `AGapInTheReplaySkipsAheadAsTheFirstRunDid`, transcript (149 m then 160 m) | pass | verified (rule); generator by trace |
| 4a | Changes passed in one frame land on the last; loops add up, since the memory holds what the HUD read | `SeveralChangesPassedInOneFrameLandOnTheLast`; `TrackMemory.Piece.Start` | pass | verified (rule); second loop by trace |
| 5 | It never reads below 0 or ahead of the truth | `ItNeverReadsBelowZeroOrRunsAhead` | pass | verified (rule) |
| 6 | Each new run starts truthful, and a rewind still waiting doesn't carry over | `ResetStartsTruthfulAgain`; `Readout.Reset()` next to both `Strip.Reset()` calls | pass | verified (rule); wiring by reading |
| 7 | In the generator, the looping door puts the HUD 110 m behind (10 remembered pieces of 10 m plus the fork) | `LoopTests.TheLoopingDoorRewindsTheReadout` (PlayMode) | not run | unverified |
| 8 | The other door leaves it true | `LoopTests.TheOtherDoorLeavesTheReadoutTrue` (PlayMode) | not run | unverified |
| 9 | Score, summary, ghost, share line and best keep the true distance | only `UIManager`'s HUD line reads the readout (`grep -rn Readout Assets/Scripts`) | by reading | verified by reading |
| 10 | Story runs never lie: they have no forks | `LoopTests.NoForksInTheStory` (from #94, not run) | not run | unverified |
| 11 | Every assembly compiles in Unity 6000.2.15f1 | batch compile, no errors | pass | verified |
| 12 | `scripts/acceptance.sh` runs from a fresh clone | run in a clean worktree of the first commit: 46 passed, 100% | pass | verified |

## Paths

| Situation | What the HUD shows | Evidence |
|---|---|---|
| no fork yet, or the other door | true distance | claims 1 and 8 |
| looping door taken, still on the fork's piece | true distance, still counting | claim 3 |
| reaches the replayed track | the reading at that stretch's start | claims 2 and 7 |
| a second loop | further behind, by that loop's stretch | claim 4 |
| die, restart, lap or new run | true again | claim 6 |

## Numbers

- Tests outside Unity: 53 passed, 0 failed (7 new).
- Coverage of the pure rules: line 100%, branch 100%. `DistanceReadout` alone: 100% / 100%.
- Complexity: `DistanceReadout` methods are 1 to 3. `TryCommit` is unchanged at 3. `SpawnNext`
  rises from 28 to 31, from laying the readout's changes with each replayed piece; splitting it
  is the next Ready item.

## Transcript

From `scripts/acceptance.sh`, the real `TrackMemory` and `DistanceReadout`, scheduled as the
generator does for each piece it lays again:

```
== The HUD distance through a looping door: 10 m pieces, an exit at piece 15, the fork is piece 20
   Exits aren't laid again, so the replay is 10 pieces over 110 m of the first run.
   true  150 m   HUD  150 m
   true  159 m   HUD  159 m
   true  160 m   HUD  160 m
   true  205 m   HUD  205 m
   true  209 m   HUD  209 m
   true  210 m   HUD   90 m
   true  260 m   HUD  140 m
   true  269 m   HUD  149 m
   true  270 m   HUD  160 m
   true  310 m   HUD  200 m
   true  400 m   HUD  290 m
```

## By hand

In a free run with the Loops flag on (the default), run to the first fork (about 200 m):

1. Note the HUD distance about 100 m before the fork.
2. Take the looping door (the one that lays familiar track). On the door's own piece the HUD
   keeps counting; as the familiar stretch starts, it drops back to roughly the reading from
   step 1.
3. Run on: the numbers count up again in step, past what you read the first time.
4. Die and look at the summary: it gives the true distance, higher than the HUD's last reading.
5. Restart: the HUD reads from 0 and tells the truth until the next looping door.
6. Take the other door at a fork: the HUD doesn't jump.

## Unverified

- Both new PlayMode tests and every older Unity suite: no licence here.
- How the jump feels: it's an instant drop with no effect. A flicker or a glitch on the
  numbers may sell it better; that's a look decision, not made here.
- Piece lengths: a replayed piece swapped for a clean one is assumed to be the same length.
  Every piece prefab's end anchor is 10 m out today; if one ever isn't, the HUD drifts by the
  difference.
- The player and the track both start at the origin (`TestSceneGenerator`); if a scene moved
  one, the rewind would land early or late by the gap.

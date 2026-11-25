# Acceptance: max/anomalies-loops (#94)

Evidence that the branch does what #94 says, for review without reading the code. Regenerate
the numbers with `scripts/acceptance.sh` (needs the .NET 8 SDK; lizard for complexity).

The Unity suites and the built game can't run on the machine this was made on (no Unity
licence), so anything that needs the game running is `unverified` below, and the pack says what
would verify it.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A story place is laps with an exit at the end of each; nothing wrong: take it, something wrong: run past | `LapRuleTests` (18), transcript 1 | pass | verified (rule) |
| 2 | A right call adds one to the floor count; a wrong one fades back to the start at 0 | `LapRuleTests.TakingACleanExitCounts`, `TakingTheExitPastAnAnomalyLosesTheCount`, transcript 1 | pass | verified (rule); fade unverified |
| 3 | You get out by taking a clean exit once the count reaches the target (2, 3, 4) | `EnoughRightCallsEndingOnATakeLeaves`, `ACountReachedByRunningPastWaitsForACleanExit` | pass | verified (rule) |
| 4 | The first lap after a start or a wrong call is clean, and only it shows the rules | `TheFirstLapIsAlwaysClean`, `AWrongCallStartsAgainOnACleanLap` | pass | verified (rule); floor text unverified |
| 5 | Dying replays the same lap rather than rerolling it | `PlacesTests.TheLapRuleFollowsThePlace` (EditMode, not run) | not run | unverified |
| 6 | Four anomalies, each in a four-piece stretch clear of both ends of the lap | `EveryKindTurnsUp`, `TheStretchStaysClearOfBothEnds`, `AWrongLapIsWrongOnlyInsideItsStretch` | pass | verified (rule); looks unverified |
| 7 | The floor count never changes while you can see it; a wrong count skips one | `TheNextLapAlreadyShowsTheCountItWillHave`, `TheCountIsHonestExceptOnAWrongCountLap` | pass | verified (rule) |
| 8 | Laps keep their length (30, 40, 50) through calm stretches | `LapTests.BreathsDontStretchALap` (PlayMode, not run) | not run | unverified |
| 9 | Arcade runs fork every 24 pieces; track beyond isn't laid until a door is taken | `LoopRuleTests` (8), `LoopTests.TheTrackHoldsAtAForkUntilADoorIsTaken` (not run), transcript 2 | pass / not run | verified (rule); generator unverified |
| 10 | The looping door lays the last 10 pieces again; the other goes on | `LoopTests.TheLoopingDoorLaysTheTrackAlreadyRun`, `TheOtherDoorGoesOn` (not run) | not run | unverified |
| 11 | No forks in the story | `LoopTests.NoForksInTheStory` (not run) | not run | unverified |
| 12 | A daily's share line: day, a strip marking each 50 m, where you got to, the link; a lost score isn't claimed | `ShareLineTests` (16), `RunStripTests` (4), transcript 3 | pass | verified |
| 13 | Copy is a button; in a browser it copies on a mouse press or a touch lift inside the button, not elsewhere | Clipboard recording (stand-in page, real jslib), its script's four checks | pass | captured (plugin only) |
| 14 | Getting out of a story place shows a card with a square per lap's call and Copy | `ShareLineTests.APlaceLineShowsEachCallAndEndsOnTheDoor`, transcript 1 | pass | verified (line); card unverified |
| 15 | The daily takes a place's look but not its rules | `PlacesTests.TheDailyTakesALookButNoRules` (not run) | not run | unverified |
| 16 | Story runs stay out of the arcade's best, stats and ghost | `StoryTests.TakingTheExitMovesOnToTheNextPlace` additions (not run) | not run | unverified |
| 17 | The page marks itself ready once the title is up | jslib run in Node | pass | verified (plugin); in a build unverified |
| 18 | Every assembly compiles in Unity 6000.2.15f1 | batch compile, no errors | pass | verified |

## Paths

### The exit rule (`LapRule.Call`)

| State | Call | Next |
|---|---|---|
| clean lap | take the exit | count +1; out if count reaches the target, else Through (fade to the next lap) |
| clean lap | run past | Wrong: count 0, the next lap is clean |
| anomaly lap | run past | Onward: count +1, the next lap carries on without a fade |
| anomaly lap | take the exit | Wrong: count 0, the next lap is clean |
| any lap | die or restart | same lap again, nothing called |

All five are walked by `LapRuleTests`; the last only by reading (death doesn't call the rule).

### Forks (`ProceduralTrackGenerator`, arcade only)

| Situation | What happens | Evidence |
|---|---|---|
| fork due, not calm, hazard gap met | a straight piece gets the wall; spawning holds | LoopTests, not run |
| fork due in a calm stretch or too close to a hazard | waits for the next free slot | `LoopRuleTests.ForksComeEveryStretch` (Due), generator by reading |
| player passes the wall on the looping side | the remembered 10 pieces are laid again | LoopTests, not run |
| player passes on the other side | fresh track | LoopTests, not run |
| player hits the middle | dies; the run resets with no fork pending | by reading |
| a replayed hazard lands in a calm stretch | swapped for a clean piece | by reading (fixed after review) |

### The run summary (`RunSummary`)

| Mode | Shows | Leaves on |
|---|---|---|
| arcade, banked or caught | score, distance, best, strip preview, Copy for a daily | Space or R after 0.6 s |
| story, got out of a place | OUT OF <place>, laps, right, wrong, call preview, Copy | Space or R after 0.6 s |
| story, died or restarted | nothing; straight back in | - |

`ShareTests` cover clicking Copy and clicking elsewhere (not run).

## Coverage

Pure rules under plain .NET (`scripts/acceptance.sh`), 46 tests:

| Class | Line | Branch |
|---|---|---|
| LapRule | 100% | 100% |
| LoopRule | 100% | 100% |
| TrackMemory | 100% | 100% |
| RunStrip | 100% | 100% |
| ShareLine | 100% | 100% |

Before this pack, `LapRule.InStretch` on a scenery anomaly, the place line's 12-square cap and
an unused `TrackMemory.Capacity` were uncovered: two tests were added and the property removed.
The Unity-side code (generator, player, summary, overlay) has no coverage number: it needs the
Unity suites, with the Code Coverage package, once there's a licence.

## Complexity

272 functions in the 40 changed C# files, mean cyclomatic complexity 3.2. Over 10, or over 60
lines (master's figure in brackets where the function existed):

| Function | CCN | Lines | Note |
|---|---|---|---|
| `ProceduralTrackGenerator.SpawnNext` | 28 (12) | 132 (70) | Laps, forks and replays all landed here. Backlog: split into picking, placing and laying once the suites run. |
| `RunSummary.Show` | 18 (14) | 59 (45) | Grew with the share line. |
| `ProceduralTrackGenerator.PickIndex` | 16 (14) | 56 (48) | The stray-exit guard. |
| `RunSummary.Update` | 15 (13) | 38 (32) | The Copy button. |
| `AttackPrompt.Update` | 15 (14) | 38 (37) | Already over on master. |
| `ProceduralTrackGenerator.ResetRun` | 11 (4) | 49 (25) | Lap and loop state. |
| `PlayerTrackMovement.AdvanceAlongTrack` | 10 (10) | 64 (64) | Unchanged in shape. |
| `PlayerTrackMovement.EndSequence` | 9 (5) | 61 (46) | The story moving on. |

## Transcripts

From the real rules (`tools/acceptance/Demo`), seeded:

```
== The office: laps of 30, 2 right calls to get out, seed 7
   The player calls right except on lap 2, where they take the exit past an anomaly.
   lap  0: floor shows 0, rules on floor, anomaly None; takes the exit -> Through, count 1
   lap  1: floor shows 3, no rules, anomaly WrongCount; runs past -> Onward, count 2
   lap  2: floor shows 2, no rules, anomaly WrongLight (pieces 16-19); takes the exit -> Wrong, count 0
   lap  3: floor shows 0, rules on floor, anomaly None; takes the exit -> Through, count 1
   lap  4: floor shows 1, no rules, anomaly HangingPillars (pieces 16-19); runs past -> Onward, count 2
   lap  5: floor shows 2, no rules, anomaly HangingPillars (pieces 14-17); runs past -> Onward, count 3
   lap  6: floor shows 5, no rules, anomaly WrongCount; runs past -> Onward, count 4
   lap  7: floor shows 4, no rules, anomaly None; takes the exit -> Leave, count 5
   calls made: right, right, wrong, right, right, right, right, right

   Share line on getting out:
     Liminal Leap · the office
     🟩🟩🟥🟩🟩🟩🟩🟩🚪
     out after 8 laps · 7 right, 1 wrong
     maxwellpark.itch.io/liminal-leap

== Forks in a free run: every 24 pieces after the first 20, seed 1234
   piece  20: fork 0, the right door lays the last 10 pieces again
   piece  44: fork 1, the left door lays the last 10 pieces again
   piece  68: fork 2, the left door lays the last 10 pieces again
   piece  92: fork 3, the left door lays the last 10 pieces again
   piece 116: fork 4, the left door lays the last 10 pieces again

== Daily share lines from the real ShareLine
   Got out at 287 m with 3,120, through an exit:
     Liminal Leap daily 2026-11-24
     ⬛🟨🟦🟥⬛🟥🚪
     the house · out at 287 m · 3,120
     maxwellpark.itch.io/liminal-leap
   Caught at 287 m after passing an exit, so the score is lost and not claimed:
     Liminal Leap daily 2026-11-24
     ⬛🟨🟦🟥⬛🟥👁
     the house · caught at 287 m
     maxwellpark.itch.io/liminal-leap
   Summary preview of that strip (rich text in the game font):
     <color=#9EA6B3>□</color><color=#E3B341>□</color><color=#4FA3E0>□</color><color=#E5534B>□</color><color=#9EA6B3>□</color><color=#E5534B>□</color>
```

Worth a look: with a target of 2, this office run took 8 laps, because getting out needs a
clean exit and anomaly laps kept coming (laps 4 to 6). That's the rule as decided, but it may
play long; the target and the 50% odds are on the decisions list.

## Captures

- `clipboard-demo.webm` (14 s, shared with the review, not committed): a stand-in page, not the
  game, running the real `ShareClipboard.jslib` against a fake canvas drawn at twice its CSS size
  and offset on the page. A click outside the button copies nothing, a click on it copies the
  exact line, and after a reset a touch tap copies it again. Its script checks all four and
  exits 0.
- The game itself: none. A WebGL build and a scripted recording need the licence.

## Hands-on checklist

About ten minutes on the Mac, after `scripts/check.sh`:

1. Title, any key: the office opens with its name, the floor count reads 0 and the two rule
   lines show. Broken if: no count, or no rules.
2. Run to the first exit and take it (E): a short fade, then the count reads 1 and the rules are
   gone. Broken if: the place ends, or the count stays 0.
3. Keep calling: look for wrong-coloured lights, pillars short of the floor, low lights or a
   count that skips. On one, run past the exit; on a clean lap, take it. Broken if: an anomaly
   shows on a lap whose rules are on the floor, or one is visible within the lap's first quarter.
4. Make a wrong call on purpose: a fade, count 0, rules back. Broken if: the count survives.
5. Get out: OUT OF THE OFFICE with laps, right and wrong, and Copy result. Click Copy: COPIED,
   and the clipboard holds the line. Broken if: Copy does nothing, or the card never leaves on
   Space.
6. Title, Enter (daily): around 24 pieces in, a wall with two doors. Go through one; if the
   next stretch looks like the one you just ran, that was the looping door. Broken if: no fork,
   no track past the wall, or the middle of the wall doesn't kill.
7. Die in the daily: the summary shows the coloured strip and Copy result. Broken if: no strip,
   or Copy does nothing.
8. Story again after the title: the count starts at 0, not where you left it.

## Unverified

Everything marked `unverified` or `not run` above, chiefly:

- every PlayMode test (`LapTests`, `LoopTests`, `ShareTests`, `StoryTests` additions) and the
  EditMode tests that need Unity types (`PlacesTests`, `ScriptedInputTests`);
- how the anomalies, the fork wall, the floor count, the summary card and the Copy button look;
- the clipboard in a real WebGL build and on a phone.

What would verify them: an activated Unity (docs/SETUP.md), then `scripts/check.sh`, the suites
with `-enableCodeCoverage`, a WebGL build checked with `web-check --ready-marker`, and a
scripted recording of the build. Until then, the hands-on checklist.

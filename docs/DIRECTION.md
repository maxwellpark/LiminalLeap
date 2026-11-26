# Direction

Where the game is going and why. Written 2026-11-24 from the issues, PR history and the
2024 concept note, since none of that was written down in one place. Update it when a
decision below gets made.

## The pitch

**Exit 8 at a sprint: the only warning is in your mirror.**

You run forward through an empty place that should be familiar. Something follows. The
only way to read what it is about to do is the rear-view mirror, and looking back costs
you the view ahead.

## The rule everything hangs off

The mirror gives information, never protection. Holding it up does nothing good on its
own (PR #71 removed the version where it pushed the pursuer back, because that made it
free and dominant). It answers a question: which lane is the attack coming down, what a
sign really said. The skill is perception under speed, not reflex.

Anything new should either make you want to look back or punish you for it.

## Where it sits in the market

Liminal space is the aesthetic: transitional places seen empty, familiar but wrong. The
Backrooms is one specific internet myth inside it, with its own fixed look and lore.

The liminal shelf on Steam in 2026 is almost all Backrooms walking sims and co-op horror.
None are runners. The runner shelf has no liminal horror. That gap is the position.

- Lead with **liminal**, not Backrooms. Office, school and a house keep us out of the
  most crowded corner and off someone else's look.
- Use "Backrooms" only as a store tag and comparison, for the search traffic.
- Comparables: The Exit 8 (anomaly spotting), short-form itch horror (streamer-led
  discovery), Temple Run and Audiosurf (run shape).

## Two modes, one front door

| | Story (PR #91) | Daily |
|---|---|---|
| Is | Office, school, house; an exit at the end of each | One place by date, same corridor for everyone |
| Shows | No HUD, no score; toasts only for prompts (an exit, hold R, the mirror) and a fork's way | Score, speed, multiplier, summary |
| Job | First impression, what streamers play | Reason to come back |

Decided 2026-11-24: the story is the front door and the daily is retention. That narrows
#75: each place decides lying signs, light, shifting and the ghost, so only the always-on
flags are left to settle.

## State, as of 2026-11-24

Built and tested on master: endless procedural corridor with fair hazard rows, the
pursuer and its mirror-read lane attacks, exits that bank the score, daily seed, calm
stretches, an outrunnable light front, a ghost of your best run, lying signs, pause menu
and run summary. About 13k lines, a few hundred tests, an allocation
harness, `scripts/check.sh`, and a 10 MB WebGL build.

Not done:

- No outside player has touched it. Most open design questions end in "needs a playtest".
- Not on itch (#74). GitHub Actions is off and the repo has no workflows, so tests, builds
  and deploys run on a machine.
- The story (#91) has not been run in Unity.
- `companyName` is now `maxwellpark` (#77), done before any public build so no player save
  is lost. It does move the local dev save.

## USPs

Built: the mirror dilemma, signs that lie ahead and tell the truth in the mirror, light you
can outrun, a ghost of your own best run, banking at exits.

Missing, best value for cost first:

1. **Anomalies you have to notice at speed (#52).** The Exit 8 half of the pitch; nothing
   delivers it yet. Start with five or six authored ones to learn whether they read at speed.
2. **A place you can't leave (#51 loops, #54 distance that lies).** The corridor only goes
   forward, so the liminality is visual. Loops make going in circles a mechanic.
3. **A shareable daily result.** In progress: a daily's summary arms a few lines to paste
   (day, a strip of blocks, where you got to, the itch link) and its Copy button copies them.
4. **A look you can put on a thumbnail.** Grain or VHS, dolly zoom on the lunge, a camera
   that drifts so you can't always see ahead. All in the 2024 notes.
5. **A through-line for the story.** "Starts weird and gets less weird, becoming human" is
   an unusual inversion and a reason for the places to be in that order.
6. **Positional sound as a second mirror.** Also an accessibility win.
7. **Gamepad and the new Input System (#7).** For Steam and Deck later, not for itch.

## Anomalies: the exit rule

Decided 2026-11-24. Each story place is run as laps, with an exit at the end of every lap.

- **Nothing wrong this lap: take the exit. Something wrong: run past it.**
- A right call adds one to the count painted on the floor at the start of each lap. A
  wrong one sends you back to the start with the count at zero.
- You get out by taking a clean exit once the count reaches the place's target: 2 in the
  office, 3 in the school, 4 in the house.
- The first lap after a start or a wrong call is always clean, so there's a baseline. The
  floor shows the rules only on that lap, so seeing them again means you got it wrong.
- Dying replays the same lap, so dying can't be used to reroll it.
- The first four anomalies: strip lights the wrong colour, pillars that stop short of the
  floor, strip lights hung low, and a count that skips a number.
- Story only. The daily keeps its exits as banking.

Status: the rule (`LapRule`) is tested and passing. The generator and player wiring
compiles in Unity, but its play-mode tests haven't run, and nobody has played it yet.
Whether these anomalies read at speed is the open question, as #52 warned.

## Roadmap

1. Run the suites for #91 and the anomaly PR in Unity, fix what breaks, merge both.
2. ~~Company name (#77), anomalies (#52), the share line.~~ Built in the anomaly PR.
3. Build what the review decided: the Copy button, the event strip, the story share line, the
   summary preview, the daily taking a place's look only, and the cloud setup.
4. Loops (#51), decided 2026-11-24 as the next feature: recognising a repeat is the same
   skill as spotting an anomaly.
5. Publish to itch (#74) from a machine with `scripts/deploy-itch.sh`, and get 5 to 10
   people playing. The always-on flags (#75) still want that playtest.

## Open decisions

None open. The ones taken are below.

## Decisions and defaults

- Default 2026-11-26 [needs-review]: the canvas renders at no more than 1.5 device pixels per CSS
  pixel. Why: a high-DPI laptop at 2 renders four times the pixels of 1, with a full-screen
  vignette and the mirror camera on top, and the fog and dim light hide the difference. To
  change: `devicePixelRatio` in the WebGL template's config.
- Default 2026-11-26 [needs-review]: a clean lap paints the place's target as a third line under
  the rules: "2 RIGHT IN A ROW    GET OUT" (3 and 4 in the school and the house). Why: the
  count had nothing to count towards, so a player who never took an exit saw no reason to.
  A wrong call still says nothing more than the count going back to 0 and the rules showing
  again. To change: `LapRule.GoalLine`, or drop the line in `LapSign.Paint`.
- Default 2026-11-26 [needs-review]: a click on the title only focuses the game; a key starts
  it. Why: on itch the click a player makes to focus the frame started the story and skipped
  the title. A tap no longer starts anything either, which costs nothing while the page is
  marked not mobile friendly (docs/ITCH.md). To change: `TitleScreen.Clicked`; a touch start
  would need its own button.
- Default 2026-11-26 [needs-review]: the speed vignette is squashed to 0.6 below the centre, so
  at speed the floor ahead stays readable (a median of 32 to 37 of 255 at the bottom centre in
  every place, from 20 to 27) while the sides and top still close in. The places' own light is
  unchanged: over 16 seconds the story's office and the office's daily track each other, and
  the earlier "much darker" was one frame of each. To change: `belowSquash` on SpeedVignette
  (1 is the old round vignette).
- Default 2026-11-26 [needs-review]: footsteps land on the running bob's low points, capped at
  3.2 a second at the start speed rising to 4.5 at full speed, which sounds as about 2.9 and 4.0
  (a jog to a sprint; the bob turns about once a metre, so most low points pass silent), at 0.3
  of the landing thud's volume, with the floor's landing pitch and the left foot 4% lower. A
  landing counts as a step. Why: on every low point they rattled at 24 a second. To change:
  `stepsAtStart` and `stepsPerSecond` on the Player prefab, `stepVolume` and `stepPitchSpread`
  on AudioManager.
- Default 2026-11-26 [needs-review]: stand-in props from primitives until generated models
  replace them: the office has a water cooler, filing cabinet, plant and photocopier; the
  school lockers, a fire extinguisher, a notice board and a bin; the house a side table with a
  lamp, a coat stand with a coat left on it, an armchair and a radiator. Real-world size, which
  reads small against the corridor (8 m of track, a 9.5 m ceiling); scaling them up is the open
  question. To change: `Assets/Scripts/Editor/PropPrefabs.cs`, then Liminal Leap > Generate
  Stand-in Props.
- Default 2026-11-25 [needs-review]: the school and the house follow the office. The school
  has speckled grey-green vinyl, painted blockwork and suspended ceiling tiles; the house has
  oak floorboards, faded floral wallpaper and a textured plaster ceiling. Floor art is per floor
  kind, so the vinyl is every linoleum stretch and the oak every boards stretch, free runs
  included. To change: the files in `Assets/Resources/Art`, made as `docs/ART.md` says.
- Decided 2026-11-25: an Art pass with locally generated textures and models, starting with the
  office: floor, wall and ceiling textures and 3 to 5 landmark props the map can record. The
  WebGL build may grow to 25 MB in all (10 MB now). A Claude session on the owner's machine
  runs the generation with max-kit's GPU skills; cloud sessions do the code, the size check
  and the browser check. Assets are committed, never generated at runtime, so a lap's baseline
  stays fixed for the exit rule. Generator metadata is stripped before commit, each model's
  licence is checked for use in the UK, and the procedural textures stay as the fallback.
- Decided 2026-11-25: the looping door gets a faint tell in the world, so a fork can be read.
- Default 2026-11-25 [needs-review]: the tell is the pillars on the looping side of the fork
  piece, which stand (which ones, and how tall) as they did on the first piece of the stretch
  that door lays again. Lights hang down the middle, so they stay as they were. To change:
  `ProceduralTrackGenerator.EchoReplay` and `TrackScenery.Echo`.
- Decided 2026-11-25: Map the space (#61) maps each story place. The design round settled:
  - Forks in a place are detours, not calls: a fork mid-lap, clear of the anomaly stretch, where
    one door carries on to the lap's exit and the other loops back over the last stretch,
    costing time while the pursuer gains. The count and the exit rule don't change.
  - Each place also hides one door that leads to a secret stretch and a second way out. Its
    content (what the stretch is, what the second ending gives) still needs designing.
  - A place's layout is fixed and the same for every player, so maps and routes can be shared.
    Anomalies still vary lap to lap.
  - The map shows only between runs: on the run summary and the place's title card.
  - It records forks and the doors taken, where anomalies were seen, and the place's floors
    and landmarks, so it reads as a floor plan.
- Default 2026-11-25 [needs-review]: a map mark is a place along the lap (the piece's position
  in it), a kind (floor, landmark, anomaly, fork, looping door, onward door) and a value. A
  door counts as known once taken either way. To change: `PlaceMap`.
- Default 2026-11-25 [needs-review]: "fixed per place" means every lap of a place is the same
  corridor, as in Exit 8, with the anomaly the only difference. It sharpens the baseline the
  exit rule already relies on, and gives the map one corridor to fill in. To change: say laps
  should differ, and each lap gets its own fixed layout instead.
- Decided 2026-11-25: the dev scene (`MovementTestScene`) stays in builds. Players can't reach
  it, it costs almost nothing, and it is the title screen's fallback and the manual test scene.
  #76 closed for this.
- Default 2026-11-25 [needs-review]: the HUD's distance lies only through a looping door. It
  rewinds to the reading at the start of the replayed stretch once the player reaches it, and
  each replayed piece reads what it read the first time, so the same numbers come round again.
  It stays behind from then on. The score, the run summary, the ghost and the share line all
  keep the true distance, so a run still compares fairly. To change: `DistanceReadout`, and
  `UIManager`, the only place that reads it.
- Decided 2026-11-25: four floors for Surfaces (#56): linoleum, carpet, wet tile, floorboards.
  Arcade runs (free and daily) change floor in stretches of 8 to 14 pieces from the run's
  seed, opening on linoleum; each story place keeps one floor (office carpet, school linoleum,
  house boards), so a floor change never reads as a wrong lap. The seam is the only warning:
  no HUD text.
- Default 2026-11-25 [needs-review]: how each floor moves you, against linoleum's 1. Carpet:
  grip 1.25, bob 0.6, a low, quiet landing. Wet tile: grip 0.35, so strafing slides in and out
  of a lane. Boards: bob 1.35, a low knock on landing. Wet tile turns up only in arcade runs.
  To change: `FloorFeel.For`, and the `Surfaces` flag turns the whole thing off.
- Decided 2026-11-25: the mood lighting keeps following the true distance through a loop, so a
  replayed stretch is lit a little differently the second time.
- Decided 2026-11-25: the pursuer's attacks (#67 to #70), forks in the story and a place's
  best on its last lap stay in Ideas.
- Default 2026-11-24 [needs-review]: loops are on in the arcade runs (daily and free) and off
  in the story: a fork every 24 pieces after the first 20, the looping door picked from the
  run's seed, replaying the last 10 pieces. Why: the story's exits are already calls. To change:
  the `Loops` flag and the generator's serialised numbers.
- Decided 2026-11-24: the pitch is "Exit 8 at a sprint: the only warning is in your mirror."
- Decided 2026-11-24: the daily takes a place's look (fog, light, name) but keeps the
  arcade's own flags, so shifting and the ghost never turn up in it unannounced. Built.

Taken without a decision, since each is cheap to change.

- Decided 2026-11-24: laps of 30, 40 and 50 pieces (`Places`).
- Default 2026-11-24 [needs-review]: 2, 3 and 4 right calls to get out, with even odds of a
  lap being wrong (`Places`).
- Default 2026-11-24 [needs-review]: the four anomalies and their four-piece stretch
  (`LapRule`, `TrackScenery`).
- Decided 2026-11-24: each 50 m square of the share strip marks what happened there (a dodged
  attack, a near miss, a stretch spent looking back), not just distance. Built: red, yellow
  and blue squares, the loudest event in a stretch winning.
- Decided 2026-11-24: the share line keeps the itch link before the page exists.
- Decided 2026-11-24: copying is a Copy button on the summary after a run, not a key. In a
  browser the page copies on the click itself, checked against the button's rectangle, so
  touch works too. Built.
- Decided 2026-11-24: getting out of a story place offers its own line (place, right and
  wrong calls, laps), with no anomaly spoilers. Built: a card on leaving a place, with a green
  or red square per lap's call and the same Copy button.
- Decided 2026-11-24: the summary previews the strip in shapes the game font has; the
  emoji stay in the copied text only. Built: coloured outline squares.
- Default 2026-11-24 [needs-review]: 1Password vault `LiminalLeap` with items `itch` and
  `unity` (`op/`).

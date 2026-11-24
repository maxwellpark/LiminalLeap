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
| Shows | No HUD, no score, no toasts | Score, speed, multiplier, summary |
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
- Not on itch (#74). GitHub Actions is off, so tests and deploys run on a machine; the
  workflows (#19, #92) aren't live.
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
   (day, a strip of blocks, where you got to, the itch link) and C copies them.
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

1. Run #91 in Unity, fix what breaks, merge.
2. ~~Company name (#77).~~ Done. The always-on flags (#75) still want a playtest.
3. Publish to itch (#74) from a machine with `scripts/deploy-itch.sh`, and get 5 to 10
   people playing.
4. Anomalies (#52) as the headline feature, with the shareable daily line alongside.
5. Loops (#51) once anomalies read, since recognising a repeat is the same skill.

## Open decisions

- Whether the daily should inherit a place's rules. Today a daily on the house turns on
  shifting, which #75 argued reads as a bug.

## Defaults to review

Taken without a decision, since each is cheap to change.

- Decided 2026-11-24: laps of 30, 40 and 50 pieces (`Places`).
- Default 2026-11-24 [needs-review]: 2, 3 and 4 right calls to get out, with even odds of a
  lap being wrong (`Places`).
- Default 2026-11-24 [needs-review]: the four anomalies and their four-piece stretch
  (`LapRule`, `TrackScenery`).
- Decided 2026-11-24: each 50 m square of the share strip marks what happened there (a dodged
  attack, a near miss, a stretch spent looking back), not just distance. To build.
- Decided 2026-11-24: the share line keeps the itch link before the page exists, and C
  stays the copy key.
- Decided 2026-11-24: getting out of a story place offers its own line (place, right and
  wrong calls, laps), with no anomaly spoilers. To build.
- Decided 2026-11-24: the summary previews the strip in shapes the game font has; the
  emoji stay in the copied text only. To build.
- Default 2026-11-24 [needs-review]: 1Password vault `LiminalLeap` with items `itch` and
  `unity` (`op/`).

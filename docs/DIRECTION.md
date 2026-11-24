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

Recommended: the story is the front door and the daily is retention. Not formally decided
yet; settling it also settles #75.

## State, as of 2026-11-24

Built and tested on master: endless procedural corridor with fair hazard rows, the
pursuer and its mirror-read lane attacks, exits that bank the score, daily seed, calm
stretches, an outrunnable light front, a ghost of your best run, lying signs, pause menu
and run summary. About 13k lines, a few hundred tests, an allocation
harness, `scripts/check.sh`, and a 10 MB WebGL build.

Not done:

- No outside player has touched it. Most open design questions end in "needs a playtest".
- Not on itch (#74). The CI deploy (#92) and CI tests (#19) wait on secrets.
- The story (#91) has not been run in Unity.
- `companyName` is still `DefaultCompany` (#77), which has to change before anyone has a save.

## USPs

Built: the mirror dilemma, signs that lie ahead and tell the truth in the mirror, light you
can outrun, a ghost of your own best run, banking at exits.

Missing, best value for cost first:

1. **Anomalies you have to notice at speed (#52).** The Exit 8 half of the pitch; nothing
   delivers it yet. Start with five or six authored ones to learn whether they read at speed.
2. **A place you can't leave (#51 loops, #54 distance that lies).** The corridor only goes
   forward, so the liminality is visual. Loops make going in circles a mechanic.
3. **A shareable daily result.** One copyable line, Wordle style. Cheapest growth there is.
4. **A look you can put on a thumbnail.** Grain or VHS, dolly zoom on the lunge, a camera
   that drifts so you can't always see ahead. All in the 2024 notes.
5. **A through-line for the story.** "Starts weird and gets less weird, becoming human" is
   an unusual inversion and a reason for the places to be in that order.
6. **Positional sound as a second mirror.** Also an accessibility win.
7. **Gamepad and the new Input System (#7).** For Steam and Deck later, not for itch.

## Roadmap

1. Run #91 in Unity, fix what breaks, merge.
2. Decide the shipped flags (#75) and the company name (#77).
3. Publish to itch (#74, #92) and get 5 to 10 people playing.
4. Anomalies (#52) as the headline feature, with the shareable daily line alongside.
5. Loops (#51) once anomalies read, since recognising a repeat is the same skill.

## Open decisions

- Story as the front door (above).
- Company name, before the first public build.
- Whether the daily should inherit a place's rules. Today a daily on the house turns on
  shifting, which #75 argued reads as a bug.

# Playtests

GOAL.md's last line wants at least five people outside the project to finish a story place.
This is how to run those sessions and what to bring back. Finding the players, and sending them
the build, are the owner's (BACKLOG.md, Blocked on owner).

## Before sending a build

1. `scripts/check-web.sh --build` passes on the commit being sent.
2. The itch page is restricted, with a password, and the build is pushed there
   (docs/ITCH.md). A public page isn't needed and isn't wanted yet.
3. Note the commit in the table below, so a report can be matched to the build it played.

## What to ask a player

Send this, or say it, and nothing more about how the game works; the first minutes are part of
what's being tested.

> It's a short horror runner in the browser. Play the story from the title screen until you
> get out of the first place, or until you've had enough. Then play today's run once (Enter on
> the title). Think out loud if you can, or jot notes as you go. When you get out, the game
> offers a line to copy: paste it back to me, with your browser and computer.

## What to watch for

Whether by watching, a recording or their notes:

- **The exit rule.** Do they work out "nothing wrong, take the exit; something wrong, run past"
  without being told? How many laps before they do? Do they misread a plain lap as wrong?
- **The anomalies.** Which ones do they spot at speed, and which do they miss? (Strip lights the
  wrong colour, pillars short of the floor, lights hung low, a count that skips a number.)
- **The mirror.** Do they look back on their own? Before an attack, or only after one hits?
- **Deaths.** Where, and to what: a hazard, the pursuer, a fork's middle.
- **The look.** Do the places read as an office, a school, a house? Is it too dark to see?
- **Comfort.** Motion, the flashing (there's a reduced flashing option on pause, F), and the
  sound.
- **Performance.** A stutter, a long load, or a browser that won't run it.

## Questions afterwards

1. What were you trying to do, in your own words?
2. When did you first notice something was wrong in a lap, and what was it?
3. What did the mirror tell you?
4. Was anything unfair, or unclear?
5. Would you play the daily again tomorrow?

## What counts

A player counts for GOAL.md's line once they paste back a place's line: "Liminal Leap · the
office", a square per lap's call, then "out after N laps". A line that says caught, or no line,
is still a useful session but doesn't count.

## Sessions

| Date | Player | Build commit | Place got out of | Notes |
|---|---|---|---|---|
| | P1 | | | |

Players are P1, P2 and so on; who they are stays outside the repo.

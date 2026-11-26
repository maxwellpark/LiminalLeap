# The itch page

Everything the itch.io page (#74) needs, so making it takes minutes. Creating the page, the
account steps and the first push are the owner's (BACKLOG.md, Blocked on owner). The images
come from a build: `scripts/build-web.sh`, then `scripts/store-images.sh`.

## Settings

Defaults for the owner to confirm; nothing here is settled until the page exists.

| Field | Value |
|---|---|
| Title | Liminal Leap |
| Project URL | maxwellpark.itch.io/liminal-leap (the share line already links here) |
| Short description | Exit 8 at a sprint: the only warning is in your mirror. (The decided pitch; whether a public page names another game is the owner's call.) |
| Classification | Games |
| Kind of project | HTML, played in the browser |
| Release status | In development |
| Genre | Action |
| Viewport | 960 x 600, with the fullscreen button on; mobile friendly off |
| Visibility | Restricted or draft until the first push has been checked |

The viewport matches the player's default web size and the size `scripts/check-web.sh` plays at.
The repo's push scripts don't check the page's visibility: `deploy-itch.sh` runs `butler push`
straight to whatever the page is. shipwright's `itch-push` refuses a public page unless asked,
and switching to it waits on the owner (BACKLOG.md, Blocked on owner). Until then, keep the page
restricted or draft before every push.

## Tags

A suggestion; itch allows ten.

horror, psychological-horror, liminal-space, runner, endless-runner, first-person, atmospheric,
short, singleplayer, unity

## Description

A draft for the owner to edit.

> You run through an empty place that should be familiar. Something follows. The only way to
> read what it is about to do is the mirror, and looking back costs you the view ahead.
>
> **Three places.** An office after hours, a school at night, a house that's almost yours. Each
> is run in laps, with an exit at the end of every one. Nothing wrong this lap: take the exit.
> Something wrong: run past it. Get enough calls right in a row and you're out.
>
> **A daily run.** The same corridor for everyone today, in one of the three places. Share the
> line it gives you; nothing is uploaded.
>
> **The mirror tells you things. It never saves you.**

## Controls

| Key | Does |
|---|---|
| A, D or the arrow keys | steer |
| Space | jump; let go early for a short hop |
| Left Shift or Q | look back in the mirror |
| E | leave through an exit |
| P or Esc | pause; then T for the title, F for reduced flashing, M to mute |
| R, held | restart |
| Enter, on the title | today's run |
| Any other key, on the title | the story |

## Images

| File | Use | Size |
|---|---|---|
| `Build/store/cover.png` | Cover image | 630 x 500 |
| `Build/store/shot-1.png` to `shot-4.png` | Screenshots: the story, then each place's daily | 1920 x 1200 |

`scripts/store-images.sh` makes them from the current build by playing real runs. They show the
game as it is, a few seconds into each run; a staged moment with an anomaly or the pursuer in
view would sell it better, and is worth picking by hand once the art settles.

## For the owner to answer

- **itch's AI disclosure.** The floor, wall and ceiling textures were generated with Stable
  Diffusion 1.5 and finished by a script (`Assets/Art/Sources.md` has each one). Answer itch's
  question in its current wording; what else to declare is the owner's call.
- **Price.** Free, or pay what you want; nothing here assumes either.
- **Content notes.** A suggestion: a pursuer, sudden attacks and dark spaces, with no gore or
  blood as far as the code shows. The pause menu has a reduced flashing option (F); whether the
  page should warn about flashing as well is the owner's call.

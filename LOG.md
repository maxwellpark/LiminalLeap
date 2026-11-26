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
- Loops, in the generator (db92197): forks in the arcade runs behind the `Loops` flag, spawning
  held at the fork until the player passes the wall, the looping door replaying the remembered
  stretch. A review found a replay could put a hazard in a breath and drift on repeat loops;
  fixed before the commit. Compiled in Unity; LoopTests not run.

End of session. Landed: the review items (daily look only, the event strip, the Copy button and
preview, the story card, the max-kit licence exception) and loops. Half done: nothing; every
change is committed. Defaults taken: loops in the arcade only, with their numbers
(docs/DIRECTION.md). Blocked on the owner: the Unity licence and 1Password setup, the shared
tools choice, the itch page, players, and pushing this branch. Next: run the suites once there's
a licence, then tick the loops line; or a design for forks in the story.

## 2026-11-24, later

- Review fixes pushed to #93: story runs kept out of the arcade's records, copy on a touch
  lifting, and the smaller findings. #93 isn't merged: neither it nor #91 has run in Unity.
- The page marks itself ready once the title is up, so `web-check --ready-marker` can tell a
  booted build from a stuck loading bar. Checked by compile and the jslib in Node; a real
  web build needs the licence.
- Audit pass by a review agent over the session's work, four lenses. No soft-lock found.
  Landed in ba64c0a: due exits now beat a breath (the house's laps had stretched to 56),
  lap state clears when a run mode is chosen, story laps no longer replace the arcade ghost,
  the floor count shows what a lap will be if the calls before it are right so it never
  changes in view, and a lap test waits for the generator's rule. 44 pure tests pass.

End of session. Ready is empty again: every unmet goal line waits on the owner (the Unity
licence and 1Password, the itch page, players), and so do merging #91 and #93 and pushing
3bcd2ab and ba64c0a. Next: run every suite once there's a licence, then tick lines.

## 2026-11-25 and 26

- Merged #94 to #107: anomalies on the exit rule, loops and the shareable result (#94); the
  distance that lies; surfaces (four floors); the SpawnNext split; the door tell; pinned and
  then removed workflows (Actions is off); the map's design and its record in the save; the
  automation Unity ID in the setup guide; the art pass's groundwork; a smaller web build; and
  generated textures for all three places.
- Closed from the last entry: #91 merged under #94, #93 was replaced by #94, and 3bcd2ab and
  ba64c0a are on master.
- WebGL builds run here without an activated licence; max-kit allows that for development
  checks. Every build since has been loaded in headless Chromium: the title, a run, and each
  place's daily with the page's clock shifted to that place's day.
- Found by checking in a browser: the pursuer's capsule class was stripped from WebGL builds
  (fixed with link.xml); the build needed its scenes generated first; walls left a wedge open
  at bends (a review caught that the first fix was half the size needed).
- Textures came from Stable Diffusion 1.5 on a CPU here, finished by `tools/art/tiles.py`;
  sources are in `Assets/Art/Sources.md`.

End of session. Defaults taken: the floors' feel, the distance readout, the door tell,
the map record's shape, every place's look (all in docs/DIRECTION.md). Blocked on the owner:
the Unity licence and 1Password, the itch page, players, the office's props (GPU). Next: the
Ready list above.

## 2026-11-26, the long session

- Merged #112 to #125, each built, checked in a browser, reviewed by a fresh agent and merged
  when green: stand-in props; footsteps per floor, at a pace that
  rises with speed; the lighting check per place (the speed vignette no longer blacks out the
  floor ahead); the title naming every key and warning of flashing; a click on the title only
  focusing the game; the page marking the title, runs and laps apart; the place's target on the
  floor; a plain load screen and failed-load message; the browser check playing the game in a
  cross-origin iframe as itch does; a 1.5 cap on the canvas pixel ratio; one generated scene for
  the web build; the title sting waiting for the browser's audio; the pursuer's lane scan no longer dropping
  hazards past 32 colliders.
- The browser check now has nine runs: the story, each place's daily, `itch-frame`, `hidpi`,
  `no-webgl2`, `title-click` and `audio`. Each new one was shown failing on the build before its
  fix. The .NET suite grew to 136 tests; Controls, VignetteTexture and the footstep cadence run
  there now.
- Two audit passes refilled Ready. Their leftovers: the pursuer's lane scan (Ready), a story
  death that says nothing (Blocked: quiet story or not), the splash screen and how itch serves
  `.gz` (Blocked).
- Traps found: a reviewer agent switched branches while a build ran, which put a commit on the
  wrong branch (fixed locally before any push; reviewers are now told not to); renumbering a
  list by first match also renumbered the map list (fixed; renumber within Ready only); the
  scene generator rewrites the tracked Build Settings (build-web.sh now puts them back).

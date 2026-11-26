# Acceptance: max/school-house-textures

The school's and the house's own floors, walls and ceilings, made the office's way, plus two
new finishing kinds in `tools/art/tiles.py`: blocks and boards.

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The school has blockwork walls, ceiling tiles and vinyl | the school's daily in the WebGL build (the page's clock shifted to 2026-12-03): blockwork reads on both walls | pass | verified (browser) |
| 2 | The house has wallpaper, a plaster ceiling and its warm light | the house's daily (2026-11-26) | pass | verified (browser) |
| 3 | The right place's art loads | a temporary log in the build: look=school, its wall and ceiling found, then removed | pass | verified |
| 4 | Vinyl is every linoleum stretch and oak every boards stretch | floor art is per floor kind (`PlaceArt.ForFloor`); both dailies open on vinyl | pass | verified (browser) |
| 5 | Blocks and boards tile | their edge columns and rows carry matching mortar and seams | pass | verified |
| 6 | No generator metadata; sources recorded | Pillow reads no info; `Assets/Art/Sources.md` has each prompt and seed | pass | verified |
| 7 | Within budget, clean console | 11 MB of 25 MB; no errors in either daily | pass | verified |

## Found on the way

- The first browser check showed the office in both "dailies": clicking to focus the page
  counted as any key on the title and started the story. The test now presses Enter without
  clicking, and `docs/ART.md` says so.
- Fine detail vanished at running distance, so the mortar is drawn 4 px wide, and the art is
  filtered anisotropically (level 8, trilinear) for walls seen at grazing angles.

## Unverified

- The oak boards in a run: dailies open on vinyl, and a boards stretch comes later.
- The builds came from an unlicensed Unity editor, which max-kit allows for development checks.

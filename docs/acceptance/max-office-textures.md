# Acceptance: max/office-textures

The office's first generated art: carpet, wall and ceiling textures, repeated every 2 m, with
floor laid out to the walls.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The office has carpet, walls and a ceiling from generated textures | WebGL build, office run in headless Chromium, screenshots | pass | verified (browser) |
| 2 | No dark trench between the floor and the walls | before: a black strip each side; after: floor runs under the pillars to the walls | pass | verified (browser) |
| 3 | Textures repeat every 2 m, so a carpet tile is 0.5 m, on the strips as on the slab | tiling in `PlaceArt`; the strips' `_MainTex_ST` scaled by 4.5/8 (review traced the cube's UVs) | pass | verified (browser, review) |
| 4 | The textures tile with no seams | each offset by half and inspected; `tiles.py` run on test images, edge columns and rows continue the lattice (review) | pass | verified |
| 5 | No generator metadata | the PNGs hold only IHDR, IDAT and IEND (review), and Pillow reads no info | pass | verified |
| 6 | Levelling reaches its brightness without clipping | a dark test image levelled to 0.42 and 0.82: means 0.42 and 0.82, maxima 167 and 234 | pass | verified |
| 7 | Within the size budget | `scripts/build-web.sh`: 10 MB of 25 MB | pass | verified |
| 8 | Where each file came from is recorded | `Assets/Art/Sources.md`: model, revision, licence clause, prompts, seeds | pass | verified |
| 9 | Free runs keep the open void | no walls without a place's art (`TrackPiece.Dress`) | by reading | verified by reading |

## By hand

1. Start the story: the office has carpet underfoot, walls behind the pillars and a ceiling.
2. Watch for the pillar anomaly: pillars stopping short of the floor still read against the
   carpet.
3. Start a free run: the corridor is the open void as before, with carpet stretches using
   the new carpet.

## Unverified

- Turns leave a dark gap between wall sections (in the backlog).
- The props, which need a GPU (on the owner's machine).
- The builds came from an unlicensed Unity editor, which max-kit allows for development checks.

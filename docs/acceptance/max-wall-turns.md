# Acceptance: max/wall-turns

At a turn, the next piece starts the full 7 degrees round, so straight 10 m walls left a wedge
open on the outside of each bend, 8.5 m x tan 7 = 1.04 m deep at the walls. A turning piece now
reaches 1.1 m past its end with its walls, ceiling and side floor. The first version reached
0.6 m, from splitting the angle between the two pieces; a review caught that.

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | No dark wedge at a bend | seven frames through the first ten seconds of the office in the WebGL build: walls continuous through the bends, where an earlier build showed a gap on the right | pass | verified (browser) |
| 2 | Only turning pieces reach, so straight runs have no overlapping faces to flicker | `PlaceArtTests.OnlyATurningPieceReachesPastItsEnd` (EditMode) | not run | unverified |
| 3 | The reach covers the gap | the next piece starts at the full 7 degrees: 8.5 m x tan 7 = 1.04 m at the walls and 1.06 m at the ceiling's corner, under 1.1 m; the test asserts it too | pass | verified (arithmetic) |
| 4 | Within budget, clean console | `scripts/build-web.sh`: 10 MB of 25 MB; no console errors in the run | pass | verified |

On the inside of a bend the reaching wall pokes 1.1 m x tan 7 = 0.14 m past the next wall,
beyond the pillars, where it shows only as a slight edge. Unverified: the EditMode test (no Unity licence for the runner), and bends sharper than 7
degrees, which no prefab has. The builds came from an unlicensed Unity editor, which max-kit
allows for development checks.

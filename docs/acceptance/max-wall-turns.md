# Acceptance: max/wall-turns

At a turn, the next piece starts 7 degrees round, so straight 10 m walls left a wedge open on
the outside of each bend, about 0.5 m wide at the walls. A turning piece now reaches 0.6 m past
its end with its walls, ceiling and side floor.

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | No dark wedge at a bend | seven frames through the first ten seconds of the office in the WebGL build: walls continuous through the bends, where an earlier build showed a gap on the right | pass | verified (browser) |
| 2 | Only turning pieces reach, so straight runs have no overlapping faces to flicker | `PlaceArtTests.OnlyATurningPieceReachesPastItsEnd` (EditMode) | not run | verified by reading |
| 3 | The reach covers the gap | 8.5 m x tan(3.5 degrees) is 0.52 m, under 0.6 m | pass | verified (arithmetic) |
| 4 | Within budget, clean console | `scripts/build-web.sh`: 10 MB of 25 MB; no console errors in the run | pass | verified |

Unverified: the EditMode test (no Unity licence for the runner), and bends sharper than 7
degrees, which no prefab has. The builds came from an unlicensed Unity editor, which max-kit
allows for development checks.

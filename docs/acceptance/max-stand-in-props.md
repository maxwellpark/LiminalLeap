# Acceptance: max/stand-in-props

Four stand-in props per place, built from primitives by an editor script, until generated
models replace them.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Each place has four props where `PlaceArt` loads them | 12 prefabs under `Assets/Resources/Art/Places/*/Props`, made by `PropPrefabs.GenerateFromCommandLine` in batch mode | pass | verified |
| 2 | No prop can be hit or fall | no Collider or Rigidbody in any prefab (grep of the prefab files) | pass | verified |
| 3 | Props stand in a run, beside the track | a temporary log in the office's daily: photocopiers and a water cooler at x = 5.2, z every few pieces; removed before committing | pass | verified (browser) |
| 4 | They read as what they are | a capture just after the office's daily starts: photocopiers with their lids and a water cooler with its bottle and tap | pass | verified (browser) |
| 5 | The build still passes | `scripts/check-web.sh --build`: four runs, 11 MB of 25 MB | pass | verified |

## Unverified

- The school's and the house's props in a run; same code path as the office's.
- Scale: at real-world size they're small against the corridor. Recorded as a default for review.
- The builds came from an unlicensed Unity editor, which max-kit allows for development checks.

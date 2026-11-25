# Acceptance: max/no-actions (#101)

Removes the GitHub workflows, which never ran with Actions off, has the web build generate its
scenes, and keeps `CapsuleCollider` through WebGL stripping.

## Claims

| # | Behaviour | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | A web build works from a fresh checkout: it generates the scenes the build lists | `scripts/build-web.sh` run here: three `GENERATED` lines, `BUILD OK`, 10 MB | pass | verified |
| 2 | A failed or partial generation stops the build | the scene count run against a real log (3, passes) and an empty one (0 of 3, refused) | pass | verified |
| 3 | The build loads in a browser and a run starts | headless Chromium: ready marker after 5 s, a key starts the office run, steering moves the view | pass | verified |
| 4 | The pursuer's capsule, built as a run starts, no longer logs a stripped class | the run's console before `link.xml`: one `CapsuleCollider` error; after: none | pass | verified |
| 5 | Nothing refers to the deleted workflows | a review's grep across the repo | pass | verified by review |
| 6 | Generated scenes are safe to delete and rebuild | `.gitignore` covers them and the generator rewrites them from a seed (review) | by reading | verified by review |

## By hand

1. On a fresh clone, run `scripts/build-web.sh`: it generates three scenes, builds, and prints
   the size against the 25 MB budget.
2. Serve `Build/WebGL` and open it: the title screen comes up; any key starts the office.
3. Start a run; the browser console stays free of errors.

## Unverified

- The builds came from an unlicensed Unity editor, which max-kit allows for development checks.

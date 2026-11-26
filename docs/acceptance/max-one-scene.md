# Acceptance: max/one-scene

The web build generates and ships only the scene the title loads.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The build lists one generated scene | `Build/build-web.log`: `BUILD START ... scenes=TitleScreen, Generated/Seed1700, MovementTestScene` (before: Seed1700, 1701 and 1702) | pass | verified (build) |
| 2 | The title still loads it, and every run starts | `scripts/check-web.sh --build`: every run | pass | verified (browser) |
| 3 | The tracked Build Settings are left as they were | `build-web.sh` copies `EditorBuildSettings.asset` aside and puts it back on exit; `git status` is clean after a build | pass | verified (build) |
| 4 | The attack prompt names the mirror keys from Controls | `AttackPrompt` builds "hold SHIFT to look" from `Controls.LookBack` | pass | by reading |

Size: 11 MB of 25 MB, unchanged, as the scenes share their assets.

## Unverified

- `check.sh --full` still generates three scenes for the Unity suites; not run here (no licence).

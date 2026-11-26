# Acceptance: max/save-robust

The save and the daily, from an audit: nothing is lost or written over that shouldn't be, and
the save stops growing a record a day.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | Reading a day's record creates nothing | `SaveDataTests.PeekingAtADayAddsNothing`; the title reads with `PeekDaily` | pass | verified (.NET) |
| 2 | Only the latest 60 days are kept, never the one just asked for | `OnlyTheLatestDaysAreKept` (400 days in, 60 kept, the newest present), `AnOldDayAskedForIsKeptEvenPastTheCap` | pass | verified (.NET) |
| 3 | A save from a newer version isn't downgraded | `ASaveFromANewerVersionIsNotDowngraded`; `SaveStore` then plays on without saving that session | pass | .NET; the store by reading |
| 4 | The daily's key and seed come from one instant | `DailyPickTests.AKeyAndItsSeedAlwaysComeFromTheSameDay`, either side of midnight UTC; `ChooseDaily` uses `Pick` | pass | verified (.NET) |
| 5 | An unreadable save is kept aside before a fresh one replaces it | `SaveStore.KeepUnreadable`: the first such text under `liminalleap.save.unreadable`, in a try/catch so a full quota can't stop play | pass | by reading |
| 6 | The save still works in a browser | `scripts/check-web.sh --build`: `save-reload` and every other run | pass | verified |

`scripts/acceptance.sh`: 141 tests pass; `DailySeed` builds under .NET now that `RunMode` has its own file.

## Unverified

- The unreadable and newer-version paths in a real browser: the check can't plant a save.
- Nothing reads the kept-aside text back yet; it's there for a person to recover by hand.

# Acceptance: max/goal-line

A clean lap paints the place's target under the rules, so the count has something to count
towards.

| # | Claim | Evidence | Result | Status |
|---|---|---|---|---|
| 1 | The line names the place's target | `LapRuleTests.TheGoalLineNamesTheTarget`: 2, 4, and "RIGHT ONCE" for 1 or 0 | pass | verified (.NET) |
| 2 | The number it gives is the number of right exits that get you out | `LapRuleTests.TheGoalLineMatchesWhenARightExitLetsYouOut`: for 1 to 4, that many right exits in a row and `Call` returns Leave | pass | verified (.NET) |
| 3 | It's on the floor, under the rules, on the story's first lap | a capture 2.5 s into the story: "ANYTHING WRONG / NOTHING WRONG / 2 RIGHT IN A ROW" in the office | pass | verified (browser) |
| 4 | Only on clean laps, as the rules are | `LapSign.Paint` shows it with the rules, and hides it with them | pass | by reading |
| 5 | The build runs with it | `scripts/check-web.sh --build`: every run, including `title-click` | pass | verified |

`scripts/acceptance.sh`: 136 tests pass.

## Unverified

- The school's 3 and the house's 4 on their floors; same code, different number.
- Whether players read it at speed: a playtest question.

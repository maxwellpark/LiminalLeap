#!/usr/bin/env bash
# The numbers in docs/acceptance: coverage of the pure rules, transcripts of what they decide,
# and complexity of every C# file changed against a base. Needs the .NET 8 SDK; lizard is optional.
#
# Usage: scripts/acceptance.sh [base]    base defaults to origin/master
set -uo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
BASE="${1:-origin/master}"
OUT="${TMPDIR:-/tmp}/liminalleap-acceptance"
rm -rf "$OUT" && mkdir -p "$OUT"
fail=0

echo "=== tests and coverage (pure rules, plain .NET)"
if dotnet test "$PROJECT/tools/acceptance/Tests" --collect:"XPlat Code Coverage" --results-directory "$OUT" >"$OUT/test.log" 2>&1; then
  grep -E "Passed!" "$OUT/test.log"
else
  grep -E "Failed|error" "$OUT/test.log" | head -20
  fail=1
fi

report="$(find "$OUT" -name coverage.cobertura.xml | head -1)"
if [ -n "$report" ]; then
  python3 - "$report" <<'PY'
import sys, xml.etree.ElementTree as ET
r = ET.parse(sys.argv[1]).getroot()
print(f"overall: line {float(r.get('line-rate'))*100:.1f}%, branch {float(r.get('branch-rate'))*100:.1f}%")
for c in r.iter('class'):
    if c.get('filename').endswith('RunOutcome.cs'):
        continue
    miss = [l.get('number') for l in c.find('lines').iter('line') if l.get('hits') == '0']
    print(f"  {c.get('name'):22s} line {float(c.get('line-rate'))*100:5.1f}%  branch {float(c.get('branch-rate'))*100:5.1f}%  uncovered: {','.join(miss) or 'none'}")
PY
fi

echo
echo "=== transcripts"
dotnet run --project "$PROJECT/tools/acceptance/Demo" 2>&1 || fail=1

echo
echo "=== complexity of C# changed against $BASE"
LIZARD="${LIZARD:-$(command -v lizard || true)}"
if [ -z "$LIZARD" ]; then
  echo "SKIP: no lizard (pip install lizard, or set LIZARD=<path>)"
else
  mapfile -t files < <(git -C "$PROJECT" diff --name-only --diff-filter=AM "$BASE"...HEAD -- 'Assets/Scripts' | grep '\.cs$')
  if [ "${#files[@]}" -gt 0 ]; then
    (cd "$PROJECT" && "$LIZARD" --csv "${files[@]}" 2>/dev/null) | python3 -c '
import csv, sys
rows = [(int(r[1]), int(r[4]), r[6].split("Assets/Scripts/")[-1], r[7]) for r in csv.reader(sys.stdin)]
print(f"{len(rows)} functions, mean CCN {sum(r[0] for r in rows)/len(rows):.1f}, max {max(r[0] for r in rows)}")
for c, l, f, n in sorted((r for r in rows if r[0] > 10 or r[1] > 60), reverse=True):
    print(f"  CCN {c:3d}  {l:3d} lines  {f}: {n}")'
  fi
fi

[ "$fail" = "0" ] && echo -e "\nACCEPTANCE DATA OK" || echo -e "\nACCEPTANCE DATA FAILED"
exit "$fail"

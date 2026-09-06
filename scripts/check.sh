#!/usr/bin/env bash
# The gate. Typecheck, both suites, scene regeneration, validator.
#
# Usage:
#   scripts/check.sh              typecheck + both test suites
#   scripts/check.sh --full       the above plus regenerate assets/scenes and validate
#   scripts/check.sh --fast       typecheck only, no Unity, runs while the editor is open
#
# Why the fast path exists: Unity holds a project lock, so nothing else here can run with
# the editor open. Typechecking goes through Unity's own Roslyn against its DLLs instead,
# which needs no lock and catches every ordinary C# error in a couple of seconds.
set -uo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY:-/Applications/Unity/Hub/Editor/6000.2.15f1/Unity.app/Contents/MacOS/Unity}"
UC="$(dirname "$(dirname "$UNITY")")"
WORK="${TMPDIR:-/tmp}/liminalleap-check"
mkdir -p "$WORK"

FULL=0
FAST=0
[ "${1:-}" = "--full" ] && FULL=1
[ "${1:-}" = "--fast" ] && FAST=1

fail=0
step() { printf '\n=== %s ===\n' "$1"; }

# ---------------------------------------------------------------- typecheck
step "typecheck"

API="$UC/UnityReferenceAssemblies/unity-4.8-api"
CSC="$UC/DotNetSdkRoslyn/csc.dll"
DOTNET="$UC/NetCoreRuntime/dotnet"
[ -x "$DOTNET" ] || DOTNET="$(command -v dotnet || true)"

if [ ! -f "$CSC" ] || [ -z "$DOTNET" ]; then
  echo "SKIP: no Roslyn or dotnet under $UC"
else
  refs=()
  for d in "$API"/mscorlib.dll "$API"/System.dll "$API"/System.Core.dll "$API"/Facades/netstandard.dll; do
    [ -f "$d" ] && refs+=("-r:$d")
  done
  for d in "$UC/Managed/UnityEngine/"UnityEngine*.dll; do refs+=("-r:$d"); done
  for d in "$PROJECT/Library/ScriptAssemblies/"*.dll; do
    case "$(basename "$d")" in LiminalLeap*) ;; *) refs+=("-r:$d") ;; esac
  done

  runtime=()
  while IFS= read -r f; do runtime+=("$f"); done \
    < <(find "$PROJECT/Assets/Scripts" -name '*.cs' -not -path '*/Editor/*')

  out="$("$DOTNET" "$CSC" -nologo -noconfig -nostdlib+ -target:library -langversion:latest \
    -out:"$WORK/runtime.dll" "${refs[@]}" "${runtime[@]}" 2>&1 | grep -E "error" | head -20)"

  if [ -n "$out" ]; then echo "$out"; fail=1; else echo "runtime OK"; fi

  erefs=("${refs[@]}")
  for d in "$UC/Managed/UnityEditor.dll" "$UC/Managed/UnityEngine/"UnityEditor*.dll; do
    [ -f "$d" ] && erefs+=("-r:$d")
  done
  erefs+=("-r:$WORK/runtime.dll")

  if [ -f "$WORK/runtime.dll" ]; then
    editor=()
    while IFS= read -r f; do editor+=("$f"); done < <(find "$PROJECT/Assets/Scripts/Editor" -name '*.cs')
    out="$("$DOTNET" "$CSC" -nologo -noconfig -nostdlib+ -target:library -langversion:latest \
      -out:"$WORK/editor.dll" "${erefs[@]}" "${editor[@]}" 2>&1 | grep -E "error" | head -20)"
    if [ -n "$out" ]; then echo "$out"; fail=1; else echo "editor OK"; fi

    NUNIT="$(find "$PROJECT/Library/PackageCache" -name nunit.framework.dll 2>/dev/null | head -1)"
    trefs=("${erefs[@]}")
    [ -n "$NUNIT" ] && trefs+=("-r:$NUNIT")

    for mode in EditMode PlayMode; do
      files=()
      while IFS= read -r f; do files+=("$f"); done < <(find "$PROJECT/Assets/Tests/$mode" -name '*.cs')
      out="$("$DOTNET" "$CSC" -nologo -noconfig -nostdlib+ -target:library -langversion:latest \
        -out:"$WORK/$mode.dll" "${trefs[@]}" "${files[@]}" 2>&1 | grep -E "error" | head -15)"
      if [ -n "$out" ]; then echo "$out"; fail=1; else echo "$mode tests OK"; fi
    done
  fi
fi

[ "$FAST" = "1" ] && { [ "$fail" = "0" ] && echo -e "\nCHECK OK (typecheck only)" || echo -e "\nCHECK FAILED"; exit "$fail"; }

# Everything past here needs the project lock.
if pgrep -f "Unity.app/Contents/MacOS/Unity" >/dev/null 2>&1; then
  echo -e "\nEditor is open, so the suites cannot run. Close it, or use --fast."
  exit 1
fi

results() {
  python3 - "$1" "$2" <<'PY'
import sys, os, xml.etree.ElementTree as ET
name, path = sys.argv[1], sys.argv[2]
if not os.path.exists(path):
    print(f"{name}: NO RESULTS (did it compile?)"); sys.exit(1)
r = ET.parse(path).getroot()
failed = int(r.get('failed') or 0)
print(f"{name}: total={r.get('total')} passed={r.get('passed')} failed={r.get('failed')}")
for tc in r.iter('test-case'):
    if tc.get('result') != 'Passed':
        print("   FAIL:", tc.get('name'))
        m = tc.find('.//message')
        if m is not None:
            print("     ", (m.text or '').strip()[:200])
sys.exit(1 if failed else 0)
PY
}

for mode in EditMode PlayMode; do
  step "$mode tests"
  "$UNITY" -batchmode -projectPath "$PROJECT" -runTests -testPlatform "$mode" \
    -testResults "$WORK/$mode.xml" -logFile "$WORK/$mode.log" -accept-apiupdate >/dev/null 2>&1
  results "$mode" "$WORK/$mode.xml" || fail=1
  grep -E "^ALLOC" "$WORK/$mode.log" 2>/dev/null
done

if [ "$FULL" = "1" ]; then
  step "assets and scenes"
  "$UNITY" -batchmode -quit -projectPath "$PROJECT" \
    -executeMethod TrackPiecePrefabs.GenerateFromCommandLine \
    -logFile "$WORK/prefabs.log" -accept-apiupdate >/dev/null 2>&1
  grep -E "MATERIALS BAKED|PIECES GENERATED" "$WORK/prefabs.log" || { echo "prefab generation failed"; fail=1; }

  rm -rf "$PROJECT/Assets/Scenes/Generated"
  "$UNITY" -batchmode -quit -projectPath "$PROJECT" \
    -executeMethod TestSceneGenerator.GenerateFromCommandLine -seed 1700 -count 3 \
    -logFile "$WORK/scenes.log" -accept-apiupdate >/dev/null 2>&1

  step "validator"
  "$UNITY" -batchmode -quit -projectPath "$PROJECT" \
    -executeMethod SceneValidator.ValidateFromCommandLine \
    -logFile "$WORK/validate.log" -accept-apiupdate >/dev/null 2>&1
  grep -E "VALIDATE" "$WORK/validate.log" || { echo "validator produced nothing"; fail=1; }
  grep -q "VALIDATE OK" "$WORK/validate.log" || fail=1
fi

if [ "$fail" = "0" ]; then
  echo -e "\nCHECK OK"
else
  echo -e "\nCHECK FAILED"
fi
exit "$fail"

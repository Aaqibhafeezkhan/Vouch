#!/usr/bin/env bash
# Runs each sample once. Every sample must fail exactly one test, Fails_on_purpose,
# with the Vouch message, and the framework must report it as a plain failure.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
out="$(mktemp -d)"
trap 'rm -rf "$out"' EXIT

expected='Expected total to be 120m, but found 100m.'
status=0

fail() {
  echo "FAIL $1: $2" >&2
  status=1
}

check_trx() {
  python3 -I - "$1" "$2" "$expected" <<'PY'
import sys
import xml.etree.ElementTree as ET

trx, sample, expected = sys.argv[1:4]
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
results = ET.parse(trx).getroot().findall(".//t:UnitTestResult", ns)
failed = [r for r in results if r.get("outcome") != "Passed"]
if len(failed) != 1:
    sys.exit(f"expected exactly one non-passing test, found {len(failed)}")
r = failed[0]
name = r.get("testName", "")
if name.split(".")[-1] != "Fails_on_purpose":
    sys.exit(f"failing test is {name}, not Fails_on_purpose")
if r.get("outcome") != "Failed":
    sys.exit(f"outcome is {r.get('outcome')}, not Failed")
msg = r.findtext(".//t:ErrorInfo/t:Message", default="", namespaces=ns)
if expected not in msg:
    sys.exit(f"message does not contain '{expected}': {msg}")
PY
}

for sample in XunitV2 XunitV3 NUnit MSTest; do
  dir="$root/$sample"
  trx="$out/$sample.trx"
  log="$out/$sample.log"
  code=0

  if [ "$sample" = XunitV3 ]; then
    dotnet run --project "$dir" -- -reporter json -result-trx "$trx" >"$log" 2>&1 || code=$?
  else
    dotnet test "$dir" --logger "trx;LogFileName=$sample.trx" --results-directory "$out" >"$log" 2>&1 || code=$?
  fi

  if [ "$code" -eq 0 ]; then
    fail "$sample" "run exited 0, the failing test did not fail"
    continue
  fi
  if [ ! -f "$trx" ]; then
    fail "$sample" "no TRX written (crash or build error), see below"
    tail -20 "$log" >&2
    continue
  fi
  if ! reason="$(check_trx "$trx" "$sample" 2>&1)"; then
    fail "$sample" "$reason"
    continue
  fi
  if [ "$sample" = XunitV3 ] && ! grep -q '"Cause":"Assertion"' "$log"; then
    fail "$sample" "xUnit v3 did not classify the failure as an assertion failure"
    continue
  fi
  echo "ok $sample"
done

exit "$status"

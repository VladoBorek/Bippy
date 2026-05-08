#!/usr/bin/env bash
set -euo pipefail

# This script runs inside Docker.
# It builds the in-process differential harness, instruments the fuzzed assemblies,
# then runs AFL++. It does not publish or start either CLI.

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

TEAM_B_PROJECT="${TEAM_B_PROJECT:-/work/Project_TEAM_B/team-34/Mnemonic/Mnemonic/Mnemonic.csproj}"
HARNESS_PROJECT="$PROJECT_ROOT/PV286.Project.DiffFuzz.Afl/PV286.Project.DiffFuzz.Afl.csproj"

BIN_ROOT="$PROJECT_ROOT/diff-bin-afl"
HARNESS_OUT="$BIN_ROOT/harness"

CORPUS_DIR="$PROJECT_ROOT/PV286.Project.DiffFuzz.Afl/Testcases"
DICT_FILE="$PROJECT_ROOT/PV286.Project.DiffFuzz.Afl/cli.dict"
FINDINGS_DIR="$PROJECT_ROOT/diff-fuzz-findings"

FUZZ_SECONDS="${FUZZ_SECONDS:-300}"
INCLUDE_DERIVE="${INCLUDE_DERIVE:-0}"
INCLUDE_DERIVE_ENTROPY="${INCLUDE_DERIVE_ENTROPY:-0}"
TIMEOUT_MS="${TIMEOUT_MS:-7000}"

if [ ! -f "$TEAM_B_PROJECT" ]; then
  echo "Team B project not found: $TEAM_B_PROJECT"
  exit 1
fi

if [ ! -f "$HARNESS_PROJECT" ]; then
  echo "Harness project not found: $HARNESS_PROJECT"
  exit 1
fi

rm -rf "$HARNESS_OUT"
mkdir -p "$HARNESS_OUT" "$FINDINGS_DIR"

echo "Publishing in-process differential harness..."
dotnet publish "$HARNESS_PROJECT" \
  -c Release \
  -f net9.0 \
  -p:TeamBProjectPath="$TEAM_B_PROJECT" \
  -o "$HARNESS_OUT"

HARNESS_DLL="$HARNESS_OUT/PV286.Project.DiffFuzz.Afl.dll"
CORE_DLL="$HARNESS_OUT/PV286.Project.DiffFuzz.Core.dll"
OUR_IMPL_DLL="$HARNESS_OUT/BusinessLayer.dll"
TEAM_B_IMPL_DLL="$HARNESS_OUT/Mnemonic.dll"

COMMON_ARGS=(
  --corpus "$CORPUS_DIR"
  --failures "$FINDINGS_DIR"
)

if [ "$INCLUDE_DERIVE" = "1" ]; then
  COMMON_ARGS+=(--include-derive)
fi

if [ "$INCLUDE_DERIVE_ENTROPY" = "1" ]; then
  COMMON_ARGS+=(--include-derive-entropy)
fi

echo "Running smoke check before SharpFuzz instrumentation..."
dotnet "$HARNESS_DLL" --smoke "${COMMON_ARGS[@]}"

echo "Instrumenting differential harness and implementations with SharpFuzz..."
sharpfuzz "$CORE_DLL"
sharpfuzz "$OUR_IMPL_DLL"
sharpfuzz "$TEAM_B_IMPL_DLL"

export AFL_SKIP_CPUFREQ=1
export AFL_I_DONT_CARE_ABOUT_MISSING_CRASHES=1
export AFL_NO_UI=1
export AFL_SKIP_BIN_CHECK=1

echo "Starting AFL++ in-process differential fuzzing for ${FUZZ_SECONDS}s..."

set +e
timeout "${FUZZ_SECONDS}s" afl-fuzz \
  -i "$CORPUS_DIR" \
  -o "$FINDINGS_DIR" \
  -x "$DICT_FILE" \
  -t "$TIMEOUT_MS" \
  -m none \
  -- dotnet "$HARNESS_DLL" "${COMMON_ARGS[@]}"
status=$?
set -e

if [ "$status" -eq 124 ]; then
  echo "Differential fuzzing finished after timeout."
elif [ "$status" -ne 0 ]; then
  echo "AFL++ exited with status $status."
  exit "$status"
fi

if find "$FINDINGS_DIR/default/crashes" -type f ! -name 'README.txt' 2>/dev/null | grep -q .; then
  echo "Differential mismatch found:"
  find "$FINDINGS_DIR/default/crashes" -type f ! -name 'README.txt'
  echo "Detailed JSON repros are in $FINDINGS_DIR"
  exit 1
fi

echo "No differential mismatches found."

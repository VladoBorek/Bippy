#!/usr/bin/env bash
set -euo pipefail

MIN_TRACE_BYTES=${MIN_TRACE_BYTES:-1024}
TRACE_OUT="./trace.nettrace"

echo ""
echo "─── Runtime Trace ──────────────────────────────"
echo "  Args: $TRACE_CLI_ARGS"
echo "───────────────────────────────────────────────"

dotnet-trace collect --output "$TRACE_OUT" -- dotnet run --project ./PV286-project/PV286-project.csproj -c Release --no-build -- $TRACE_CLI_ARGS || CLI_EXIT=$?

echo ""
echo "  Trace file  : $TRACE_OUT"
echo "  CLI exit    : ${CLI_EXIT:-0}"
echo "  Trace size  : $(stat -c%s "$TRACE_OUT") bytes"
echo "───────────────────────────────────────────────"

if [ "${CLI_EXIT:-0}" -ne 0 ]; then
  echo -e "\n\033[31m✖ CLI exited with code $CLI_EXIT\033[0m"
  exit 1
fi

size=$(stat -c%s "$TRACE_OUT")
if [ "$size" -lt "$MIN_TRACE_BYTES" ]; then
  echo -e "\n\033[31m✖ Trace too small (${size}B) — process likely crashed\033[0m"
  exit 1
fi

echo -e "\n\033[32m✔ Runtime trace passed\033[0m"
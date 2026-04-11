#!/usr/bin/env bash
set -euo pipefail

MIN_TRACE_BYTES=${MIN_TRACE_BYTES:-1024}
TRACE_OUT="./trace.nettrace"

echo ""
echo "─── Runtime Trace ──────────────────────────────"
echo "  Args: $TRACE_CLI_ARGS"
echo "───────────────────────────────────────────────"

dotnet-trace collect \
  --output "$TRACE_OUT" \
  -- dotnet run -c Release -- $TRACE_CLI_ARGS

size=$(stat -c%s "$TRACE_OUT")
echo ""
echo "  Trace file : $TRACE_OUT"
echo "  Size       : ${size} bytes"
echo "───────────────────────────────────────────────"

if [ "$size" -lt "$MIN_TRACE_BYTES" ]; then
  echo -e "\n\033[31m✖ Trace too small (${size}B) — process likely crashed\033[0m"
  exit 1
fi

echo -e "\n\033[32m✔ Runtime trace passed\033[0m"
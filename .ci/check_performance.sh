#!/usr/bin/env bash
set -euo pipefail

BINARY="${BINARY:-./release/PV286-project}"
OUTPUT="${OUTPUT:-./perf_output}"
PERF_THRESHOLD="${PERF_THRESHOLD:-5.0}"

mkdir -p "$OUTPUT"

echo "=== perf stat ==="
perf stat \
    --repeat 5 \
    -e task-clock,context-switches,page-faults \
    "$BINARY" "$@" \
    2>&1 | tee "$OUTPUT/stat_summary.txt"

# Extract elapsed time and check threshold
ELAPSED=$(grep "seconds time elapsed" "$OUTPUT/stat_summary.txt" \
          | awk '{print $1}' | tail -1)

echo ""
echo "Wall-clock: ${ELAPSED}s  (threshold: ${PERF_THRESHOLD}s)"
if awk "BEGIN { exit !(${ELAPSED} > ${PERF_THRESHOLD}) }"; then
    echo "FAIL: ${ELAPSED}s > ${PERF_THRESHOLD}s"
    exit 1
fi
echo "OK"
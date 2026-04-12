#!/usr/bin/env bash
set -euo pipefail

BINARY="${BINARY:-./release/PV286-project}"
OUTPUT="${OUTPUT:-./perf_output}"
PERF_THRESHOLD="${PERF_THRESHOLD:-5.0}"

mkdir -p "$OUTPUT"

echo "=== time ==="
/usr/bin/time -v "$BINARY" "$@" 2>&1 | tee "$OUTPUT/stat_summary.txt"

ELAPSED=$(grep "Elapsed (wall clock)" "$OUTPUT/stat_summary.txt" \
          | awk -F'[:]' '{n=split($0,a,":"); if(n==4) print a[3]*60+a[4]; else print a[2]*60+a[3]}')

echo ""
echo "Wall-clock: ${ELAPSED}s  (threshold: ${PERF_THRESHOLD}s)"
if awk "BEGIN { exit !(${ELAPSED} > ${PERF_THRESHOLD}) }"; then
    echo "FAIL: ${ELAPSED}s > ${PERF_THRESHOLD}s"
    exit 1
fi
echo "OK"

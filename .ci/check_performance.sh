#!/usr/bin/env bash
set -euo pipefail

BINARY="${BINARY:-./release/PV286-project}"
OUTPUT="${OUTPUT:-./perf_output}"
PERF_THRESHOLD="${PERF_THRESHOLD:-5.0}"

mkdir -p "$OUTPUT"

echo "=== time ==="
/usr/bin/time -v "$BINARY" "$@" 2>&1 | tee "$OUTPUT/stat_summary.txt"

ELAPSED=$(grep "Elapsed (wall clock)" "$OUTPUT/stat_summary.txt" \
          | grep -oE '[0-9]+:[0-9]+(\.[0-9]+)?' \
          | tail -1 \
          | awk -F: '{if(NF==3) print $1*3600+$2*60+$3; else print $1*60+$2}')

echo ""
echo "Wall-clock: ${ELAPSED}s  (threshold: ${PERF_THRESHOLD}s)"
if awk "BEGIN { exit !(${ELAPSED} > ${PERF_THRESHOLD}) }"; then
    echo "FAIL: ${ELAPSED}s > ${PERF_THRESHOLD}s"
    exit 1
fi
echo "OK"
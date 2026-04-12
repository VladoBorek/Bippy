#!/usr/bin/env bash
set -euo pipefail

BINARY="${BINARY:-/app/release/PV286-project}"
OUTPUT="${OUTPUT:-/app/perf_output}"
FLAMEGRAPH_DIR="${FLAMEGRAPH_DIR:-/opt/FlameGraph}"
PERF_THRESHOLD="${PERF_THRESHOLD:-3.0}"

mkdir -p "$OUTPUT"

# Allow perf inside the privileged container
echo -1 > /proc/sys/kernel/perf_event_paranoid 2>/dev/null || true
echo  0 > /proc/sys/kernel/kptr_restrict       2>/dev/null || true

echo "════════════════════════════════════════════════════════"
echo "  Dynamic Analysis — PV286-project"
echo "  Binary : $BINARY"
echo "  Args   : $*"
echo "  Output : $OUTPUT"
echo "════════════════════════════════════════════════════════"

# ── 1. perf stat ─────────────────────────────────────────────────────────────
# Collects hardware counters: cycles, instructions, cache refs/misses, branches
echo ""
echo "▶ [1/3] perf stat"

perf stat \
    --detailed \
    --repeat 5 \
    -e cycles,instructions,cache-references,cache-misses,\
branch-instructions,branch-misses,\
L1-dcache-loads,L1-dcache-load-misses,\
LLC-loads,LLC-load-misses \
    "$BINARY" "$@" \
    2>&1 | tee "$OUTPUT/stat_summary.txt"

echo "    → written: $OUTPUT/stat_summary.txt"

# ── 2. perf record ───────────────────────────────────────────────────────────
# -F 999   : sample at ~999 Hz
# -g       : capture call graphs (frame-pointer unwinding)
# --call-graph dwarf  would be better but requires debug symbols; use fp here
echo ""
echo "▶ [2/3] perf record"

perf record \
    -F 999 \
    -g \
    --call-graph fp \
    -o "$OUTPUT/perf.data" \
    -- "$BINARY" "$@"

# Human-readable flat + call-chain report
perf report \
    --input="$OUTPUT/perf.data" \
    --stdio \
    --no-children \
    2>&1 | tee "$OUTPUT/perf_report.txt"

echo "    → written: $OUTPUT/perf.data"
echo "    → written: $OUTPUT/perf_report.txt"

# ── 3. Flamegraph ─────────────────────────────────────────────────────────────
echo ""
echo "▶ [3/3] FlameGraph"

perf script \
    --input="$OUTPUT/perf.data" \
    2>/dev/null \
| "$FLAMEGRAPH_DIR/stackcollapse-perf.pl" \
| "$FLAMEGRAPH_DIR/flamegraph.pl" \
    --title "PV286-project — CPU Flamegraph" \
    --width 1600 \
    > "$OUTPUT/flamegraph.svg"

echo "    → written: $OUTPUT/flamegraph.svg"

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "════════════════════════════════════════════════════════"
echo "  Analysis complete.  Artefacts in $OUTPUT :"
ls -lh "$OUTPUT"
echo "════════════════════════════════════════════════════════"

# Validate performance
if [ -f "$OUTPUT/stat_summary.txt" ]; then
    # GNU perf format
    RUNTIME=$(grep "seconds time elapsed" "$OUTPUT/stat_summary.txt" | grep -o -E '[0-9]+[.,][0-9]+' | head -n 1 | tr ',' '.')
    if [ -n "$RUNTIME" ]; then
        echo "Elapsed Time: $RUNTIME seconds (Threshold: $PERF_THRESHOLD)"
        is_terrible=$(awk -v t1="$RUNTIME" -v t2="$PERF_THRESHOLD" 'BEGIN{print (t1>t2)?1:0}')
        if [ "$is_terrible" -eq 1 ]; then
            echo "ERROR: Performance is terrible! Execution took $RUNTIME seconds, limit is $PERF_THRESHOLD."
            exit 1
        else
            echo "Performance is acceptable."
        fi
    else
        echo "WARNING: Could not parse elapsed time from stat_summary.txt but analysis finished."
    fi
else
    echo "WARNING: $OUTPUT/stat_summary.txt not found. Cannot automatically determine performance."
fi
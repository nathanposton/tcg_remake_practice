#!/bin/bash
# Unity Test Execution and Evaluation Script for Tiny Chao Garden
# This script implements the systematic procedure for running and evaluating Unity tests

UNITY_PATH="/Applications/Unity/Hub/Editor/6000.1.13f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/nposton/tcg_remake_practice"
TIMESTAMP=$(date +%s)
OUT_DIR="$PROJECT_PATH/TestLogs"
mkdir -p "$OUT_DIR"
RESULTS_EDIT="$OUT_DIR/TestResults_EditMode-${TIMESTAMP}.xml"
RESULTS_PLAY="$OUT_DIR/TestResults_PlayMode-${TIMESTAMP}.xml"
LOG_EDIT="$OUT_DIR/test_EditMode_${TIMESTAMP}.log"
LOG_PLAY="$OUT_DIR/test_PlayMode_${TIMESTAMP}.log"
COMPILE_LOG="$OUT_DIR/compile_check_${TIMESTAMP}.log"
RUN_EDIT=true
RUN_PLAY=true
NO_KILL=false
LIST_FAILURES=false

echo "=== Unity Test Execution Script ==="
echo "Project: $PROJECT_PATH"
echo "Timestamp: $TIMESTAMP"
echo ""

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]
Options:
  --edit-only           Run EditMode tests only
  --play-only           Run PlayMode tests only
  --outdir DIR          Write logs/results to DIR (default: TestLogs)
  --no-kill             Do not auto-close Unity before running
  --list-failures       Print failing test full names after summary
  -h, --help            Show this help
EOF
}

while [ $# -gt 0 ]; do
  case "$1" in
    --edit-only) RUN_PLAY=false ;;
    --play-only) RUN_EDIT=false ;;
    --outdir) shift; OUT_DIR="$1"; mkdir -p "$OUT_DIR";;
    --no-kill) NO_KILL=true ;;
    --list-failures) LIST_FAILURES=true ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1"; usage; exit 2 ;;
  esac
  shift
done

RESULTS_EDIT="$OUT_DIR/TestResults_EditMode-${TIMESTAMP}.xml"
RESULTS_PLAY="$OUT_DIR/TestResults_PlayMode-${TIMESTAMP}.xml"
LOG_EDIT="$OUT_DIR/test_EditMode_${TIMESTAMP}.log"
LOG_PLAY="$OUT_DIR/test_PlayMode_${TIMESTAMP}.log"
COMPILE_LOG="$OUT_DIR/compile_check_${TIMESTAMP}.log"

close_unity_for_project() {
  echo "Preflight: checking for running Unity for this project..."
  local MATCH="MacOS/Unity.*-projectPath $PROJECT_PATH"
  local PIDS
  PIDS=$(pgrep -f "$MATCH" || true)
  if [ -n "$PIDS" ]; then
    echo "Found Unity process(es) for this project: $PIDS"
    echo "Attempting graceful quit via AppleScript..."
    osascript -e 'tell application "Unity" to quit' >/dev/null 2>&1 || true
    echo "Sending SIGTERM to project-specific Unity process(es)..."
    kill $PIDS >/dev/null 2>&1 || true
    # Wait up to 20 seconds for Unity to close
    for i in {1..20}; do
      sleep 1
      pgrep -f "$MATCH" >/dev/null 2>&1 || { echo "Unity closed."; return 0; }
    done
    echo "Unity did not close gracefully; forcing termination (SIGKILL)..." >&2
    pkill -9 -f "$MATCH" >/dev/null 2>&1 || true
    # Small delay to ensure processes exit
    sleep 1
  else
    echo "No Unity instance detected for this project."
  fi
}

# Attempt to close any running Unity instance for this project (unless disabled)
if [ "$NO_KILL" = false ]; then
  close_unity_for_project
else
  echo "Skipping Unity auto-close (--no-kill)"
fi

# Phase 1: Pre-Execution Validation
echo "Phase 1: Verifying project compilation..."
$UNITY_PATH -batchmode -quit \
  -projectPath "$PROJECT_PATH" \
  -logFile "$COMPILE_LOG"

COMPILE_EXIT_CODE=$?
echo "Compilation exit code: $COMPILE_EXIT_CODE"

if [ $COMPILE_EXIT_CODE -ne 0 ]; then
  echo "❌ ERROR: Project compilation failed"
  echo "Check compile_check_${TIMESTAMP}.log for details"
  exit 1
fi

echo "✅ Project compilation successful"
echo ""

# Phase 2: Test Discovery and Execution
TEST_EXIT_CODE_EDIT=0
TEST_EXIT_CODE_PLAY=0

if [ "$RUN_EDIT" = true ]; then
  echo "Phase 2: Running Unity tests (EditMode)..."
  $UNITY_PATH -batchmode \
    -projectPath "$PROJECT_PATH" \
    -runTests -testPlatform EditMode \
    -assemblyNames TinyChaoGarden.EditModeTests \
    -testResults "$RESULTS_EDIT" \
    -logFile "$LOG_EDIT"
  TEST_EXIT_CODE_EDIT=$?
  echo "EditMode test execution exit code: $TEST_EXIT_CODE_EDIT"
  echo ""
else
  echo "Skipping EditMode tests (--play-only)"
fi

if [ "$RUN_PLAY" = true ]; then
  echo "Phase 2: Running Unity tests (PlayMode)..."
  $UNITY_PATH -batchmode \
    -projectPath "$PROJECT_PATH" \
    -runTests -testPlatform PlayMode \
    -assemblyNames TinyChaoGarden.PlayModeTests \
    -testResults "$RESULTS_PLAY" \
    -logFile "$LOG_PLAY"
  TEST_EXIT_CODE_PLAY=$?
  echo "PlayMode test execution exit code: $TEST_EXIT_CODE_PLAY"
  echo ""
else
  echo "Skipping PlayMode tests (--edit-only)"
fi

# Phase 3: Results Evaluation
echo "Phase 3: Evaluating test results..."

# Fallback helper: if result file missing, try to copy Unity default result location
fallback_result_copy() {
  local MODE="$1"; shift
  local DEST="$1"; shift
  local DEFAULT_PATH="$HOME/Library/Application Support/DefaultCompany/tcg_remake_practice/TestResults.xml"
  if [ ! -f "$DEST" ] && [ -f "$DEFAULT_PATH" ]; then
    echo "Note: $MODE results not found at expected path. Copying from default Unity path." >&2
    cp "$DEFAULT_PATH" "$DEST"
  fi
}

parse_results() {
  local FILE="$1"; shift
  local COUNT PASSED FAILED
  if [ -f "$FILE" ]; then
    if command -v xmllint >/dev/null 2>&1; then
      COUNT=$(xmllint --xpath "string(/test-run/@testcasecount)" "$FILE" 2>/dev/null)
      PASSED=$(xmllint --xpath "string(/test-run/@passed)" "$FILE" 2>/dev/null)
      FAILED=$(xmllint --xpath "string(/test-run/@failed)" "$FILE" 2>/dev/null)
    else
      COUNT=$(grep -o 'testcasecount="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1)
      PASSED=$(grep -o 'passed="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1)
      FAILED=$(grep -o 'failed="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1)
    fi
  fi
  echo "${COUNT:-0} ${PASSED:-0} ${FAILED:-0}"
}

list_failures() {
  local FILE="$1"; shift
  if [ -f "$FILE" ]; then
    # Grep-based fallback: list failed test fullnames
    grep -o '<test-case[^>]*result="Failed"[^>]*fullname="[^"]\+"' "$FILE" \
      | sed -E 's/.*fullname="([^"]+)".*/\1/'
  fi
}

summarize_mode() {
  local MODE="$1" FILE="$2" LOG="$3" EXIT_CODE="$4"
  # Try fallback copy if file is missing (Unity UI default location)
  [ -f "$FILE" ] || fallback_result_copy "$MODE" "$FILE"
  echo "--- $MODE ---" >&2
  if [ -f "$FILE" ]; then
    echo "✅ Results: $(basename "$FILE")" >&2
    read COUNT PASSED FAILED < <(parse_results "$FILE")
    echo "Tests discovered: $COUNT" >&2
    echo "Tests passed: $PASSED" >&2
    echo "Tests failed: $FAILED" >&2
    if [ "$LIST_FAILURES" = true ] && [ "${FAILED:-0}" -gt 0 ]; then
      echo "Failing tests:" >&2
      list_failures "$FILE" | sed 's/^/  - /' >&2
    fi
    local DEBUG_LOGS=$(grep -c "\[TEST\]" "$LOG" 2>/dev/null || echo "0")
    echo "Debug log entries found: $DEBUG_LOGS" >&2
    if [ "$DEBUG_LOGS" -gt 0 ]; then
      echo "Sample debug logs:" >&2; grep "\[TEST\]" "$LOG" | head -3 >&2
    fi
    echo "" >&2
    echo "$COUNT $FAILED"
  else
    echo "❌ No results generated (exit code: $EXIT_CODE)" >&2
    if [ -f "$LOG" ]; then
      local COMPILE_ERRORS
      COMPILE_ERRORS=$(grep -c "error CS" "$LOG" 2>/dev/null || true)
      COMPILE_ERRORS=${COMPILE_ERRORS:-0}
      if [ "$COMPILE_ERRORS" -gt 0 ] 2>/dev/null; then
        echo "Found $COMPILE_ERRORS compilation errors:" >&2; grep "error CS" "$LOG" | head -5 >&2
      fi
    fi
    echo "0 0"
  fi
}

COUNT_E=0; FAILED_E=0
COUNT_P=0; FAILED_P=0
if [ "$RUN_EDIT" = true ]; then
  read COUNT_E FAILED_E < <(summarize_mode "EditMode" "$RESULTS_EDIT" "$LOG_EDIT" "$TEST_EXIT_CODE_EDIT")
fi
if [ "$RUN_PLAY" = true ]; then
  read COUNT_P FAILED_P < <(summarize_mode "PlayMode" "$RESULTS_PLAY" "$LOG_PLAY" "$TEST_EXIT_CODE_PLAY")
fi

TOTAL=$((COUNT_E + COUNT_P))
FAILED_TOTAL=$((FAILED_E + FAILED_P))

echo "=== SUMMARY ==="
echo "Total tests discovered: $TOTAL"
echo "Total tests failed: $FAILED_TOTAL"

# Create/update helpful symlinks to latest outputs
ln -snf "$RESULTS_EDIT" "$OUT_DIR/latest_edit.xml" 2>/dev/null || true
ln -snf "$RESULTS_PLAY" "$OUT_DIR/latest_play.xml" 2>/dev/null || true
ln -snf "$LOG_EDIT" "$OUT_DIR/latest_edit.log" 2>/dev/null || true
ln -snf "$LOG_PLAY" "$OUT_DIR/latest_play.log" 2>/dev/null || true

if [ "$TOTAL" -eq 0 ]; then
  echo "❌ ISSUE: No tests discovered in either mode"
  exit 1
elif [ "$FAILED_TOTAL" -gt 0 ]; then
  echo "❌ FAILURE: $FAILED_TOTAL tests failed"
  exit 1
else
  echo "✅ SUCCESS: All $TOTAL tests passed"
  exit 0
fi

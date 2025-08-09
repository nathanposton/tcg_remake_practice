#!/usr/bin/env bash
# Unity Test Execution and Evaluation Script for Tiny Chao Garden (cross-platform)
# - Detects platform and Unity path, or use UNITY_PATH env/flag
# - Runs EditMode/PlayMode tests in headless mode
# - Writes logs/XML to TestLogs and summarizes results

set -euo pipefail

# Defaults (overridden by env or flags)
UNITY_VERSION_DEFAULT="6000.1.13f1"
UNITY_VERSION="${UNITY_VERSION:-$UNITY_VERSION_DEFAULT}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH_DEFAULT="$SCRIPT_DIR"
PROJECT_PATH="${PROJECT_PATH:-$PROJECT_PATH_DEFAULT}"
TIMESTAMP=$(date +%s)
OUT_DIR="${OUT_DIR:-$PROJECT_PATH/TestLogs}"
RUN_EDIT=true
RUN_PLAY=true
NO_KILL=false
LIST_FAILURES=false
ADD_NOGRAPHICS=true
PARSE_ONLY=false

mkdir -p "$OUT_DIR"

RESULTS_EDIT="$OUT_DIR/TestResults_EditMode-${TIMESTAMP}.xml"
RESULTS_PLAY="$OUT_DIR/TestResults_PlayMode-${TIMESTAMP}.xml"
LOG_EDIT="$OUT_DIR/test_EditMode_${TIMESTAMP}.log"
LOG_PLAY="$OUT_DIR/test_PlayMode_${TIMESTAMP}.log"
COMPILE_LOG="$OUT_DIR/compile_check_${TIMESTAMP}.log"

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]
Options:
  --edit-only             Run EditMode tests only
  --play-only             Run PlayMode tests only
  --outdir DIR            Write logs/results to DIR (default: TestLogs)
  --no-kill               Do not auto-close Unity before running
  --list-failures         Print failing test full names after summary
  --unity-path PATH       Explicit path to Unity editor executable
  --project-path PATH     Explicit Unity project path (default: script dir)
  --unity-version VER     Unity version (default: $UNITY_VERSION_DEFAULT)
  --no-nographics         Do not add -nographics (useful on macOS with GPU)
  --parse-only            Do not invoke Unity; summarize existing results in OUT_DIR
  -h, --help              Show this help
Environment variables (alternatives to flags):
  UNITY_PATH, PROJECT_PATH, OUT_DIR, UNITY_VERSION
EOF
}

UNITY_PATH_INPUT="${UNITY_PATH:-}"

while [ $# -gt 0 ]; do
  case "$1" in
    --edit-only) RUN_PLAY=false ;;
    --play-only) RUN_EDIT=false ;;
    --outdir) shift; OUT_DIR="$1"; mkdir -p "$OUT_DIR" ;;
    --no-kill) NO_KILL=true ;;
    --list-failures) LIST_FAILURES=true ;;
    --unity-path) shift; UNITY_PATH_INPUT="$1" ;;
    --project-path) shift; PROJECT_PATH="$1" ;;
    --unity-version) shift; UNITY_VERSION="$1" ;;
    --no-nographics) ADD_NOGRAPHICS=false ;;
    --parse-only) PARSE_ONLY=true ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage; exit 2 ;;
  esac
  shift
done

RESULTS_EDIT="$OUT_DIR/TestResults_EditMode-${TIMESTAMP}.xml"
RESULTS_PLAY="$OUT_DIR/TestResults_PlayMode-${TIMESTAMP}.xml"
LOG_EDIT="$OUT_DIR/test_EditMode_${TIMESTAMP}.log"
LOG_PLAY="$OUT_DIR/test_PlayMode_${TIMESTAMP}.log"
COMPILE_LOG="$OUT_DIR/compile_check_${TIMESTAMP}.log"

OS_NAME=$(uname -s)

parse_results_quick() {
  local FILE="$1"; shift
  local COUNT=0 FAILED=0
  if [ -f "$FILE" ]; then
    if command -v xmllint >/dev/null 2>&1; then
      COUNT=$(xmllint --xpath "string(/test-run/@testcasecount)" "$FILE" 2>/dev/null)
      FAILED=$(xmllint --xpath "string(/test-run/@failed)" "$FILE" 2>/dev/null)
    else
      COUNT=$(grep -o 'testcasecount="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1 || echo 0)
      FAILED=$(grep -o 'failed="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1 || echo 0)
    fi
  fi
  echo "${COUNT:-0} ${FAILED:-0}"
}

list_failures_quick() {
  local FILE="$1"; shift
  if [ -f "$FILE" ]; then
    grep -o '<test-case[^>]*result="Failed"[^>]*fullname="[^"]\+"' "$FILE" \
      | sed -E 's/.*fullname="([^"]+)".*/\1/'
  fi
}

find_latest_by_glob() {
  local pattern="$1"; shift
  local latest
  latest=$(ls -1t $pattern 2>/dev/null | head -1 || true)
  echo "$latest"
}

if [ "$PARSE_ONLY" = true ]; then
  echo "Parse-only mode: summarizing existing results in $OUT_DIR"
  # Determine files to parse
  LATEST_EDIT="$OUT_DIR/latest_edit.xml"
  LATEST_PLAY="$OUT_DIR/latest_play.xml"
  [ -f "$LATEST_EDIT" ] || LATEST_EDIT="$(find_latest_by_glob "$OUT_DIR/TestResults_EditMode-*.xml")"
  [ -f "$LATEST_PLAY" ] || LATEST_PLAY="$(find_latest_by_glob "$OUT_DIR/TestResults_PlayMode-*.xml")"

  COUNT_E=0; FAILED_E=0
  COUNT_P=0; FAILED_P=0
  if [ "$RUN_EDIT" = true ] && [ -n "$LATEST_EDIT" ]; then
    echo "--- EditMode ---" >&2
    read COUNT_E FAILED_E < <(parse_results_quick "$LATEST_EDIT")
    echo "Tests discovered: $COUNT_E" >&2
    echo "Tests failed: $FAILED_E" >&2
    if [ "$LIST_FAILURES" = true ] && [ "$FAILED_E" -gt 0 ]; then
      echo "Failing tests:" >&2
      list_failures_quick "$LATEST_EDIT" | sed 's/^/  - /' >&2
    fi
  fi
  if [ "$RUN_PLAY" = true ] && [ -n "$LATEST_PLAY" ]; then
    echo "--- PlayMode ---" >&2
    read COUNT_P FAILED_P < <(parse_results_quick "$LATEST_PLAY")
    echo "Tests discovered: $COUNT_P" >&2
    echo "Tests failed: $FAILED_P" >&2
    if [ "$LIST_FAILURES" = true ] && [ "$FAILED_P" -gt 0 ]; then
      echo "Failing tests:" >&2
      list_failures_quick "$LATEST_PLAY" | sed 's/^/  - /' >&2
    fi
  fi

  TOTAL=$((COUNT_E + COUNT_P))
  FAILED_TOTAL=$((FAILED_E + FAILED_P))
  echo "=== SUMMARY ==="
  echo "Total tests discovered: $TOTAL"
  echo "Total tests failed: $FAILED_TOTAL"
  if [ "$TOTAL" -eq 0 ]; then
    echo "❌ ISSUE: No tests discovered (parse-only)"
    exit 1
  elif [ "$FAILED_TOTAL" -gt 0 ]; then
    echo "❌ FAILURE: $FAILED_TOTAL tests failed"
    exit 1
  else
    echo "✅ SUCCESS: All $TOTAL tests passed"
    exit 0
  fi
fi

find_unity() {
  if [ -n "$UNITY_PATH_INPUT" ] && [ -x "$UNITY_PATH_INPUT" ]; then
    echo "$UNITY_PATH_INPUT"; return 0
  fi
  case "$OS_NAME" in
    Darwin)
      local mac_guess="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
      [ -x "$mac_guess" ] && { echo "$mac_guess"; return 0; } || true
      ;;
    Linux)
      local candidates=(
        "$HOME/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity"
        "/opt/unity/Editor/Unity"
        "/usr/bin/unity-editor"
        "/usr/local/bin/unity-editor"
      )
      for c in "${candidates[@]}"; do [ -x "$c" ] && { echo "$c"; return 0; }; done
      if command -v unity-editor >/dev/null 2>&1; then echo "$(command -v unity-editor)"; return 0; fi
      ;;
    *) ;;
  esac
  echo ""; return 1
}

UNITY_PATH_RESOLVED="$(find_unity || true)"
if [ -z "$UNITY_PATH_RESOLVED" ]; then
  echo "ERROR: Could not locate Unity editor executable." >&2
  echo "Provide via --unity-path or set UNITY_PATH, or install Unity ${UNITY_VERSION}." >&2
  exit 2
fi

close_unity_for_project() {
  echo "Preflight: checking for running Unity for this project..."
  local MATCH
  if [ "$OS_NAME" = "Darwin" ]; then
    MATCH="MacOS/Unity.*-projectPath $PROJECT_PATH"
  else
    MATCH="Unity.*-projectPath $PROJECT_PATH"
  fi
  local PIDS
  PIDS=$(pgrep -f "$MATCH" || true)
  if [ -n "$PIDS" ]; then
    echo "Found Unity process(es) for this project: $PIDS"
    if [ "$OS_NAME" = "Darwin" ]; then
      osascript -e 'tell application "Unity" to quit' >/dev/null 2>&1 || true
    fi
    echo "Sending SIGTERM to project-specific Unity process(es)..."
    kill $PIDS >/dev/null 2>&1 || true
    for i in {1..20}; do
      sleep 1
      pgrep -f "$MATCH" >/dev/null 2>&1 || { echo "Unity closed."; return 0; }
    done
    echo "Unity did not close gracefully; forcing termination (SIGKILL)..." >&2
    pkill -9 -f "$MATCH" >/dev/null 2>&1 || true
    sleep 1
  else
    echo "No Unity instance detected for this project."
  fi
}

run_unity_cmd() {
  local mode="$1"; shift
  local args=( -batchmode -projectPath "$PROJECT_PATH" "$@" )
  if [ "$ADD_NOGRAPHICS" = true ] && [ "$OS_NAME" = Linux ]; then
    args=( -nographics "${args[@]}" )
  fi
  "$UNITY_PATH_RESOLVED" "${args[@]}"
}

printf "=== Unity Test Execution Script ===\n"
printf "Project: %s\n" "$PROJECT_PATH"
printf "Unity: %s\n" "$UNITY_PATH_RESOLVED"
printf "Timestamp: %s\n\n" "$TIMESTAMP"

if [ "$NO_KILL" = false ]; then
  close_unity_for_project
else
  echo "Skipping Unity auto-close (--no-kill)"
fi

echo "Phase 1: Verifying project compilation..."
run_unity_cmd compile -quit -logFile "$COMPILE_LOG" || true
COMPILE_EXIT_CODE=$?
echo "Compilation exit code: $COMPILE_EXIT_CODE"
if [ $COMPILE_EXIT_CODE -ne 0 ]; then
  echo "❌ ERROR: Project compilation failed"
  echo "Check $(basename "$COMPILE_LOG") for details in $OUT_DIR"
  exit 1
fi

echo "✅ Project compilation successful\n"

TEST_EXIT_CODE_EDIT=0
TEST_EXIT_CODE_PLAY=0

if [ "$RUN_EDIT" = true ]; then
  echo "Phase 2: Running Unity tests (EditMode)..."
  run_unity_cmd EditMode -runTests -testPlatform EditMode \
    -assemblyNames TinyChaoGarden.EditModeTests \
    -testResults "$RESULTS_EDIT" \
    -logFile "$LOG_EDIT" || true
  TEST_EXIT_CODE_EDIT=$?
  echo "EditMode test execution exit code: $TEST_EXIT_CODE_EDIT\n"
else
  echo "Skipping EditMode tests (--play-only)"
fi

if [ "$RUN_PLAY" = true ]; then
  echo "Phase 2: Running Unity tests (PlayMode)..."
  run_unity_cmd PlayMode -runTests -testPlatform PlayMode \
    -assemblyNames TinyChaoGarden.PlayModeTests \
    -testResults "$RESULTS_PLAY" \
    -logFile "$LOG_PLAY" || true
  TEST_EXIT_CODE_PLAY=$?
  echo "PlayMode test execution exit code: $TEST_EXIT_CODE_PLAY\n"
else
  echo "Skipping PlayMode tests (--edit-only)"
fi

echo "Phase 3: Evaluating test results..."

fallback_result_copy() {
  local MODE="$1"; shift
  local DEST="$1"; shift
  local DEFAULT_PATH=""
  if [ "$OS_NAME" = "Darwin" ]; then
    DEFAULT_PATH="$HOME/Library/Application Support/DefaultCompany/tcg_remake_practice/TestResults.xml"
  else
    DEFAULT_PATH="$HOME/.config/unity3d/DefaultCompany/tcg_remake_practice/TestResults.xml"
  fi
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
      COUNT=$(grep -o 'testcasecount="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1 || echo 0)
      PASSED=$(grep -o 'passed="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1 || echo 0)
      FAILED=$(grep -o 'failed="[0-9]*"' "$FILE" | grep -o '[0-9]*' | head -1 || echo 0)
    fi
  fi
  echo "${COUNT:-0} ${PASSED:-0} ${FAILED:-0}"
}

list_failures() {
  local FILE="$1"; shift
  if [ -f "$FILE" ]; then
    grep -o '<test-case[^>]*result="Failed"[^>]*fullname="[^"]\+"' "$FILE" \
      | sed -E 's/.*fullname="([^"]+)".*/\1/'
  fi
}

summarize_mode() {
  local MODE="$1" FILE="$2" LOG="$3" EXIT_CODE="$4"
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

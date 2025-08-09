# Unity Test Suite Execution and Evaluation Procedure

## Purpose
Provide a robust, repeatable, and documented process for running, interpreting, and iterating on the Unity test suite so future agents can rely on it and focus on improving the suite and implementing game functionality.

Key principles:
- Prioritize reliable, procedural game development and testing.
- Follow the procedure as a strong default; deviate if it advances the primary objectives.
- Keep documentation, plans, and procedures up-to-date as part of every change.

## Requirements
- Unity: 6000.1.13f1
- Packages: Unity Test Framework 2.0.3
- Shell script: `/run_tests.sh` (auto-closes Unity, runs tests, parses/prints results)
- Output directory: `TestLogs/`

## Test Assemblies and Structure
- EditMode tests: `Assets/Tests/EditMode` (assembly: `TinyChaoGarden.EditModeTests`)
- PlayMode tests: `Assets/Tests/PlayMode` (assembly: `TinyChaoGarden.PlayModeTests`)
- Test utilities: `Assets/Tests/TestUtilities` (assembly: `TinyChaoGarden.TestUtilities`)

## Running the Suite
Prefer using the script (handles process control, output locations, parsing, and summaries):

```bash
# From project root
bash ./run_tests.sh

# Options
bash ./run_tests.sh \
 --edit-only        # run EditMode only \
 --play-only        # run PlayMode only \
 --outdir LogsDir   # change output directory (default: TestLogs) \
 --no-kill          # do not auto-close Unity before running \
 --list-failures    # enumerate failing test full names in summary
```

The script will:
- Gracefully close Unity for this project (AppleScript + SIGTERM; SIGKILL if needed) unless `--no-kill` is used.
- Validate compilation (batch mode) and bail if it fails.
- Run EditMode and/or PlayMode tests with explicit assembly names.
- Write logs and XML results into `TestLogs/` with a timestamp and create `latest_*.{xml,log}` symlinks.
- Parse results and print:
  - Human-readable summary to stderr (counts, optional failing test names, sample [TEST] logs)
  - Numeric totals to stdout for automated consumers
- Exit non-zero if zero tests are discovered or any tests fail.

## Outputs
- `TestLogs/test_EditMode_<ts>.log` and `TestLogs/test_PlayMode_<ts>.log`
- `TestLogs/TestResults_EditMode-<ts>.xml` and `TestLogs/TestResults_PlayMode-<ts>.xml`
- `TestLogs/compile_check_<ts>.log`
- Symlinks to latest: `TestLogs/latest_edit.xml`, `TestLogs/latest_play.xml`, `TestLogs/latest_edit.log`, `TestLogs/latest_play.log`

If Unity writes results to its default path (e.g., during UI runs), the script copies: `~/Library/Application Support/DefaultCompany/tcg_remake_practice/TestResults.xml` into `TestLogs/` as a fallback.

## Interpreting Results
The script prints a final summary and exits with:
- Exit 0: All tests passed (EditMode + PlayMode counts > 0, failures = 0)
- Exit 1: Any failure OR zero tests discovered OR test run error

Use the XML to review specifics:
- Discovered count: `/test-run/@testcasecount`
- Aggregates: `/test-run/@passed`, `/test-run/@failed`
- Failing tests: `<test-case result="Failed" fullname="...">` entries
- Logs: search `[TEST]` in the corresponding `test_*.log` for helpful context

## Troubleshooting / Decision Tree

1) Script cannot close Unity / process conflict
- Symptom: compile phase aborts with multiple instance message
- Action: Re-run without `--no-kill` (default), or manually quit Unity; if persistent, run `pkill -f "MacOS/Unity.*-projectPath <path>"` (may require password in some environments)

2) Zero tests discovered in a mode (COUNT = 0)
- Check the XML exists (see `TestLogs/latest_*.xml`). If missing, inspect `compile_check_*.log` and mode log for errors.
- Verify mode assembly definitions:
  - `TinyChaoGarden.EditModeTests.asmdef` / `TinyChaoGarden.PlayModeTests.asmdef`
  - Correct references to `TinyChaoGarden` and `TinyChaoGarden.TestUtilities`
  - Define constraint `UNITY_INCLUDE_TESTS`
  - For PlayMode, ensure required engine packages are referenced (e.g., `Unity.InputSystem`)
- Confirm test classes are public, have `[Test]`/`[UnityTest]`, and reside under correct folders.
- Try re-running only the affected mode: `bash ./run_tests.sh --edit-only` or `--play-only`.

3) Test failures present
- Use `--list-failures` to list failing test full names.
- Open the XML and associated logs to review the failure messages and stack traces.
- For Animator/scene errors (e.g., parameters missing), validate scene setup, prefabs, and Animator Controller parameters.
- Fix the code or test (see Improving the Suite below), then re-run the affected mode for faster feedback.

4) Crashes or Unity exits with code 2 or higher
- Inspect `compile_check_*.log` and `test_*.log` for stack traces.
- Re-run a single mode to isolate.
- If reproducible and not caused by tests, try clearing `Library/` (Unity will reimport) or restarting machine.
- Check Unity version/package compatibility.

## Improving the Suite and Game (Process Guidance)
Follow a TDD-first loop for each feature/system:
1. Write or update tests describing intended behavior (unit, integration, scene, gameplay tiers).
2. Run the suite (prefer single-mode runs for speed).
3. Implement or adjust code to make tests pass.
4. Run the full suite.
5. Refactor for clarity/maintainability.

Use existing reference documents for context and priorities:
- Current features implemented (working scripts) and remaining systems are summarized in project memories and docs (Chao stats, store, minigames, ring economy, dialogue, weeds, items).
- Prioritize reliability: stabilize failing tests before adding new ones.
- For PlayMode tests involving scenes, maintain deterministic setup utilities in `TestUtilities` (fixtures, input simulation, scene loaders).

When to add/expand tests:
- New feature implementation start
- Bug fix reproduction (add failing test first)
- Regression prevention after refactors

## Documentation and Planning Updates
Keep these always current when the suite, scripts, or priorities change:
- This file: `Assets/Tests/TEST_EXECUTION_PROCEDURE.md` (usage, options, outputs, troubleshooting)
- `run_tests.sh` (options help in `usage()` must match docs)
- Project plan (milestones, next objectives, status of failing/passing suites)
- Any per-system design or test strategy docs (e.g., notes for Chao stats, store, minigames)

Recommended cadence:
- After any change to testing infra or test behavior, update this doc in the same PR/commit.
- After each test run that meaningfully changes status (e.g., failures resolved/added), update the plan with a brief note of current pass/fail counts and next actions.

## Examples

Run everything and list failures:
```bash
bash ./run_tests.sh --list-failures
```

Run only PlayMode and keep editor open:
```bash
bash ./run_tests.sh --play-only --no-kill
```

Change output directory:
```bash
bash ./run_tests.sh --outdir ./MyLogs --list-failures
```

## Notes
- The script intentionally omits `-quit` for `-runTests` calls to avoid premature exit on some Unity versions.
- Results are parsed via `xmllint` when available, otherwise via grep fallbacks.
- Manual UI runs store XML in the Unity default path; the script copies this into `TestLogs/` as a fallback when needed.

## Platform and Environment Notes

- Unity version: ensure 6000.1.13f1 is installed.
- macOS:
  - Unity path typically `/Applications/Unity/Hub/Editor/6000.1.13f1/Unity.app/Contents/MacOS/Unity`.
  - The script can gracefully quit Unity via AppleScript.
- Linux:
  - Common paths: `$HOME/Unity/Hub/Editor/6000.1.13f1/Editor/Unity`, `/opt/unity/Editor/Unity`, or `unity-editor` in PATH.
  - The script adds `-nographics` automatically for headless runs; override with `--no-nographics`.

You can always provide the path explicitly:

```bash
bash ./run_tests.sh --unity-path "/absolute/path/to/Unity" --list-failures
```

## CI Enablement (GitHub Actions)

A workflow is provided at `.github/workflows/unity-tests.yml` using `game-ci/unity-test-runner`.

- Add repository secret `UNITY_LICENSE` to enable headless activation.
- The job runs both EditMode and PlayMode, capturing artifacts into `TestLogs/`.

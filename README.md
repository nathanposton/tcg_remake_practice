# Tiny Chao Garden Remake (Unity 6)

A Unity 6 re-creation of the Tiny Chao Garden 2D experience. This repository is prepared for iterative, test-driven development by humans and AI agents.

## Quickstart

- Requirements: Unity 6000.1.13f1
- Open the project in Unity Hub, or run tests headlessly using the script below.

### Run Tests (Headless)

```bash
# Preferred: auto-detects platform, logs to TestLogs/
bash ./run_tests.sh --list-failures

# If Unity is not auto-detected, specify the editor path explicitly
bash ./run_tests.sh --unity-path "/path/to/Unity" --list-failures

# EditMode only
bash ./run_tests.sh --edit-only

# PlayMode only
bash ./run_tests.sh --play-only
```

Environment variables supported: `UNITY_PATH`, `PROJECT_PATH`, `OUT_DIR`, `UNITY_VERSION`.

### CI (GitHub Actions)

This repository includes a workflow to run EditMode and PlayMode tests on every push/PR using `game-ci/unity-test-runner`. To enable:

1. Obtain a Unity license for CI.
2. Add `UNITY_LICENSE` secret to the repository.
3. Push to `main`/`master` or open a PR to trigger.

Artifacts (logs and XML results) are uploaded from `TestLogs/`.

## Development Process

- Tests are maintained under `Assets/Tests/` with EditMode and PlayMode assemblies.
- Follow the procedure in `Assets/Tests/TEST_EXECUTION_PROCEDURE.md`.
- Known working systems and scripts are documented inline and validated by tests.

## AI Handoff

See `docs/AI_HANDOFF.md` for the exact step-by-step loop for agents: run tests, analyze output, decide next test/feature edits, request human assistance for Unity GUI-only tasks (scenes/prefabs/animator hookups) with precise instructions.

## Progress Log

High-level run summaries and milestones should be recorded in `PROGRESS.md`.
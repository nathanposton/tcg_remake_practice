# AI Handoff Guide

This repository is structured so an agent can reliably run tests, interpret results, and iterate until the game is playable. Follow this loop:

## Loop (Repeat)

1. Run tests headlessly
   - Prefer: `bash ./run_tests.sh --list-failures`
   - EditMode only for fast iterations, then PlayMode, then both.
2. Analyze outputs in `TestLogs/`
   - XML: `latest_edit.xml`, `latest_play.xml`
   - Logs: `latest_edit.log`, `latest_play.log`
   - Focus on first failures per system, not all at once.
3. Decide next change
   - Add/adjust a test first (EditMode for logic, PlayMode for scenes/behaviors).
   - Implement code to satisfy the test.
   - Keep changes narrowly scoped; prefer clarity.
4. Re-run affected tests, then full suite
   - Use `--edit-only`/`--play-only` to iterate quickly.
5. Update docs and progress
   - Record pass/fail counts and key notes in `PROGRESS.md`.
   - Update `Assets/Tests/TEST_EXECUTION_PROCEDURE.md` if behavior/options changed.

## Escalate to Human (Unity GUI Required)

Request help only when strictly necessary, and include precise Unity 6 steps:

- Scene/prefab hookups (missing references, Animator Controller parameters)
- Importing/linking visual/audio assets
- Creating Input Actions assets or updating Input System settings
- Complex physics/animation tuning requiring visual inspection

Provide a checklist like:

- Open scene `Garden`.
- Select object `Chao` → add component `ChaoController`.
- In Animator Controller `ChaoAnimator`, create trigger `Blink`.
- Assign prefab `EmoteBall` to `EmoteBallFollow.target` field.
- Save scene and commit.

## Conventions

- Tests must accompany features.
- Keep `run_tests.sh` options in sync with docs.
- Favor deterministic tests; use `TestUtilities` helpers.
- Avoid long-running PlayMode tests; cap waits and assert early.
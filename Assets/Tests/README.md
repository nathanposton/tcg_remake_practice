# Tiny Chao Garden Testing Suite

This directory contains the comprehensive testing suite for the Tiny Chao Garden remake project.

## Test Structure

### EditMode Tests (`/EditMode/`)
- **BaselineTests.cs**: Unit tests for core data models (ChaoStats, SaveSystem)
- Fast execution, no Unity Play Mode required
- Tests data validation, serialization, and core logic

### PlayMode Tests (`/PlayMode/`)
- **SceneAndMovementTests.cs**: Integration tests for scenes, movement, and UI
- Requires Unity Play Mode to test actual GameObject behavior
- Tests scene transitions, input handling, animation systems

### Test Utilities (`/TestUtilities/`)
- **TestHelpers.cs**: Common utility methods for testing
- Input simulation, movement validation, test data creation
- Reduces code duplication across test files

## Running Tests

### Via Unity Test Runner
1. Open Unity Test Runner: `Window > General > Test Runner`
2. Select `EditMode` tab for unit tests
3. Select `PlayMode` tab for integration tests
4. Click `Run All` or select specific tests to run

### Via Command Line (CI/CD)
```bash
# Run EditMode tests
Unity -batchmode -runTests -testPlatform EditMode -testResults results_editmode.xml

# Run PlayMode tests  
Unity -batchmode -runTests -testPlatform PlayMode -testResults results_playmode.xml
```

## Test Categories

### Current Features (Baseline)
- ✅ Scene Management (StartScreen → Garden transition)
- ✅ Movement System (Input → Chao movement + animations)
- ✅ EmoteBall Following
- ✅ UI Systems (Blinking components, scene transitions)
- ✅ Data Models (ChaoStats validation, SaveSystem)

### Future Test Additions
As each new system is implemented, corresponding tests will be added:

- **Phase 1**: ChaoController integration, stats decay, UI panels
- **Phase 2**: Garden management, weed spawning, petting mechanics
- **Phase 3**: Store system, ring economy, item purchasing
- **Phase 4**: Minigames, scene transitions, scoring
- **Phase 5**: Dialogue system, item interactions, polish features

## Test Development Guidelines

### Test-Driven Development Process
1. **Write Test First**: Define expected behavior before implementation
2. **Red-Green-Refactor**: Failing test → implementation → passing test → refactor
3. **Regression Testing**: Run full suite after each change
4. **Coverage Validation**: Ensure critical paths are tested

### Test Naming Convention
- `MethodName_Scenario_ExpectedResult()`
- Example: `ChaoStats_DefaultInitialization_SetsCorrectValues()`

### Test Organization
- Group related tests in the same test class
- Use descriptive test names and comments
- Include Arrange-Act-Assert structure in tests
- Use TestHelpers for common operations

## Continuous Integration

This test suite is designed to integrate with CI/CD pipelines:
- EditMode tests run quickly for fast feedback
- PlayMode tests provide comprehensive validation
- Test results are output in standard formats
- All tests must pass before merging code changes

## Troubleshooting

### Common Issues
- **Input System**: Ensure Input System package is installed and configured
- **Scene Loading**: Verify scene names match exactly in tests
- **Component References**: Check that required components exist in test scenes
- **Timing**: Use appropriate wait times for async operations

### Test Debugging
- Use `Debug.Log()` statements in tests for debugging
- Run individual tests to isolate issues
- Check Unity Console for detailed error messages
- Verify test scene setup matches expectations

## Scripted Execution

For automated, headless runs use the root script:

```bash
bash ./run_tests.sh --list-failures
# If Unity is not auto-detected on your platform:
bash ./run_tests.sh --unity-path "/path/to/Unity" --list-failures
```

- On Linux the script adds `-nographics` by default; disable with `--no-nographics` if needed.
- Outputs land in `TestLogs/` with `latest_*.{xml,log}` symlinks for convenience.

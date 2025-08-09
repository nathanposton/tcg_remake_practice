using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Tests.TestUtilities
{
    /// <summary>
    /// Utility methods for testing Tiny Chao Garden functionality.
    /// Provides common test operations like input simulation, movement validation, and test data creation.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Simulates a key press and release for testing input handling.
        /// </summary>
        /// <param name="key">The key to simulate</param>
        /// <param name="duration">How long to hold the key (in seconds)</param>
        public static IEnumerator SimulateKeyPress(Key key, float duration = 0.1f)
        {
            var keyboard = InputSystem.GetDevice<Keyboard>();
            if (keyboard != null)
            {
                // Press the key
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                InputSystem.Update();
            
                // Hold for specified duration
                yield return new WaitForSeconds(duration);
            
                // Release the key
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }
        
            // Small delay to allow input processing
            yield return new WaitForSeconds(0.05f);
        }

        /// <summary>
        /// Simulates multiple key presses in sequence.
        /// </summary>
        /// <param name="keys">Array of keys to press in sequence</param>
        /// <param name="delayBetween">Delay between each key press</param>
        public static IEnumerator SimulateKeySequence(Key[] keys, float delayBetween = 0.1f)
        {
            foreach (var key in keys)
            {
                yield return SimulateKeyPress(key, 0.05f);
                yield return new WaitForSeconds(delayBetween);
            }
        }

        /// <summary>
        /// Validates that a GameObject has moved by at least the specified distance.
        /// </summary>
        /// <param name="initialPosition">Starting position</param>
        /// <param name="currentPosition">Current position</param>
        /// <param name="minimumDistance">Minimum distance that should have been moved</param>
        /// <returns>True if movement meets minimum distance requirement</returns>
        public static bool ValidateMovement(Vector3 initialPosition, Vector3 currentPosition, float minimumDistance = 0.01f)
        {
            var distance = Vector3.Distance(initialPosition, currentPosition);
            return distance >= minimumDistance;
        }

        /// <summary>
        /// Validates that a movement direction matches the expected direction within a tolerance.
        /// </summary>
        /// <param name="initialPosition">Starting position</param>
        /// <param name="currentPosition">Current position</param>
        /// <param name="expectedDirection">Expected movement direction (normalized)</param>
        /// <param name="tolerance">Angle tolerance in degrees</param>
        /// <returns>True if movement direction is within tolerance</returns>
        public static bool ValidateMovementDirection(Vector3 initialPosition, Vector3 currentPosition, Vector3 expectedDirection, float tolerance = 45f)
        {
            var actualMovement = (currentPosition - initialPosition).normalized;
            var angle = Vector3.Angle(actualMovement, expectedDirection.normalized);
            return angle <= tolerance;
        }

        /// <summary>
        /// Creates a test ChaoStats object with specified or default values.
        /// </summary>
        /// <param name="name">Chao name</param>
        /// <param name="rings">Number of rings</param>
        /// <param name="mood">Mood level (0-1)</param>
        /// <param name="belly">Belly level (0-1)</param>
        /// <returns>Configured ChaoStats object</returns>
        public static Models.ChaoStats CreateTestChaoStats(string name = "TestChao", int rings = 100, float mood = 1f, float belly = 1f)
        {
            return new Models.ChaoStats
            {
                name = name,
                rings = rings,
                mood = mood,
                belly = belly,
                stage = Models.ChaoStage.Child,
                swim = 10,
                fly = 10,
                run = 10,
                power = 10,
                stamina = 10
            };
        }

        /// <summary>
        /// Creates a test ChaoStats object with randomized stat values for testing variety.
        /// </summary>
        /// <param name="name">Chao name</param>
        /// <returns>ChaoStats with randomized values within valid ranges</returns>
        public static Models.ChaoStats CreateRandomTestChaoStats(string name = "RandomChao")
        {
            return new Models.ChaoStats
            {
                name = name,
                rings = Random.Range(0, 1000),
                mood = Random.Range(0f, 1f),
                belly = Random.Range(0f, 1f),
                stage = (Models.ChaoStage)Random.Range(0, 4),
                swim = Random.Range(0, 100),
                fly = Random.Range(0, 100),
                run = Random.Range(0, 100),
                power = Random.Range(0, 100),
                stamina = Random.Range(0, 100)
            };
        }

        /// <summary>
        /// Finds a component of the specified type in the scene, with optional name filtering.
        /// </summary>
        /// <typeparam name="T">Component type to find</typeparam>
        /// <param name="gameObjectName">Optional GameObject name to filter by</param>
        /// <returns>Found component or null</returns>
        public static T FindComponentInScene<T>(string gameObjectName = null) where T : Component
        {
            var components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return components.Length > 0 ? components[0] : null;
            }
        
            foreach (var component in components)
            {
                if (component.gameObject.name.Contains(gameObjectName))
                {
                    return component;
                }
            }
        
            return null;
        }

        /// <summary>
        /// Waits for a scene to be fully loaded and ready for testing.
        /// </summary>
        /// <param name="additionalWaitTime">Additional wait time after scene load</param>
        public static IEnumerator WaitForSceneReady(float additionalWaitTime = 0.1f)
        {
            // Wait one frame for scene to initialize
            yield return null;
        
            // Wait additional time if specified
            if (additionalWaitTime > 0)
            {
                yield return new WaitForSeconds(additionalWaitTime);
            }
        }

        /// <summary>
        /// Logs test progress with consistent formatting.
        /// </summary>
        /// <param name="testName">Name of the test</param>
        /// <param name="message">Progress message</param>
        /// <param name="isStart">True if this is the start of a test, false for progress/completion</param>
        public static void LogTestProgress(string testName, string message, bool isStart = false)
        {
            var prefix = isStart ? "[TEST START]" : "[TEST]";
            Debug.Log($"{prefix} {testName}: {message}");
        }
    }
}

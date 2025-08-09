using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    /// <summary>
    /// Play Mode tests for scene management, movement system, and UI functionality.
    /// These tests require Unity's Play Mode to test actual GameObject behavior.
    /// </summary>
    public class SceneAndMovementTests
    {
        private const string StartSceneName = "StartScreen";
        private const string GardenSceneName = "Garden";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log($"[TEST] Setting up test - Loading {StartSceneName}");
            // Load the start screen for each test
            yield return SceneManager.LoadSceneAsync(StartSceneName);
            yield return new WaitForSeconds(0.1f); // Allow scene to fully load
            Debug.Log($"[TEST] Setup complete - {StartSceneName} loaded");
        }

        [UnityTest]
        public IEnumerator StartScreen_LoadsCorrectly()
        {
            Debug.Log("[TEST] Starting StartScreen_LoadsCorrectly test");
        
            // Assert
            Assert.AreEqual(StartSceneName, SceneManager.GetActiveScene().name, 
                $"Expected scene {StartSceneName} to be loaded");
        
            // Check for essential components
            var pressStartScript = Object.FindAnyObjectByType<PressStartScript>();
            var startScreenScript = Object.FindAnyObjectByType<StartScreenScript>();
            var blinkingComponent = Object.FindAnyObjectByType<MakeComponentBlink>();
        
            Assert.IsNotNull(pressStartScript, "PressStartScript should be present in StartScreen");
            Assert.IsNotNull(startScreenScript, "StartScreenScript should be present in StartScreen");
            Assert.IsNotNull(blinkingComponent, "MakeComponentBlink should be present in StartScreen");
        
            Debug.Log("[TEST] StartScreen_LoadsCorrectly test PASSED");
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressStart_TransitionsToGarden()
        {
            Debug.Log("[TEST] Starting PressStart_TransitionsToGarden test");
        
            // Arrange
            var pressStartScript = Object.FindAnyObjectByType<PressStartScript>();
            Assert.IsNotNull(pressStartScript, "PressStartScript not found");
        
            // Act - Simulate space key press
            var keyboard = InputSystem.GetDevice<Keyboard>();
            if (keyboard != null)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                InputSystem.Update();
                yield return new WaitForSeconds(0.1f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }
        
            // Wait for scene transition
            yield return new WaitForSeconds(2f);
        
            // Assert
            Assert.AreEqual(GardenSceneName, SceneManager.GetActiveScene().name,
                $"Expected scene to transition to {GardenSceneName}");
            
            Debug.Log("[TEST] PressStart_TransitionsToGarden test PASSED");
        }

        [UnityTest]
        public IEnumerator BlinkingComponent_AnimatesCorrectly()
        {
            Debug.Log("[TEST] Starting BlinkingComponent_AnimatesCorrectly test");
        
            // Arrange
            var blinkingComponent = Object.FindAnyObjectByType<MakeComponentBlink>();
            Assert.IsNotNull(blinkingComponent, "MakeComponentBlink component not found");
        
            var targetObject = blinkingComponent.gameObject;
            var initialAlpha = GetAlphaValue(targetObject);
        
            // Act - Wait for blink animation to occur
            yield return new WaitForSeconds(1.5f); // Wait longer than blink interval
        
            var midCycleAlpha = GetAlphaValue(targetObject);
        
            // Assert - Alpha should have changed during blink cycle
            Assert.AreNotEqual(initialAlpha, midCycleAlpha, 
                "Alpha should change during blink animation");
            
            Debug.Log("[TEST] BlinkingComponent_AnimatesCorrectly test PASSED");
        }

        private float GetAlphaValue(GameObject obj)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                return spriteRenderer.color.a;
            
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                return canvasGroup.alpha;
            
            return 1f; // Default if no alpha component found
        }
    }

    /// <summary>
    /// Tests for the movement system and Chao behavior in the Garden scene.
    /// </summary>
    public class GardenMovementTests
    {
        private const string GardenSceneName = "Garden";
        private Chao_Pet.MovementLogicScript movementScript;
        private GameObject chaoObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log($"[TEST] Setting up Garden test - Loading {GardenSceneName}");
            // Load the garden scene
            yield return SceneManager.LoadSceneAsync(GardenSceneName);
            yield return new WaitForSeconds(0.1f);
        
            // Find the Chao with movement script
            movementScript = Object.FindAnyObjectByType<Chao_Pet.MovementLogicScript>();
            if (movementScript != null)
            {
                chaoObject = movementScript.gameObject;
            }
            Debug.Log($"[TEST] Garden setup complete - MovementScript found: {movementScript != null}");
        }

        [UnityTest]
        public IEnumerator Garden_LoadsCorrectly()
        {
            Debug.Log("[TEST] Starting Garden_LoadsCorrectly test");
        
            // Assert
            Assert.AreEqual(GardenSceneName, SceneManager.GetActiveScene().name,
                $"Expected {GardenSceneName} scene to be loaded");
            Assert.IsNotNull(movementScript, "MovementLogicScript should be present in Garden scene");
            Assert.IsNotNull(chaoObject, "Chao GameObject should be found");
        
            Debug.Log("[TEST] Garden_LoadsCorrectly test PASSED");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Movement_RespondsToInput()
        {
            Debug.Log("[TEST] Starting Movement_RespondsToInput test");
        
            // Arrange
            Assert.IsNotNull(movementScript, "MovementLogicScript required for this test");
            var initialPosition = chaoObject.transform.position;
        
            // Act - Simulate right arrow key press
            var keyboard = InputSystem.GetDevice<Keyboard>();
            if (keyboard != null)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.RightArrow));
                InputSystem.Update();
                yield return new WaitForSeconds(0.5f); // Allow movement
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }
        
            yield return new WaitForSeconds(0.1f);
        
            // Assert
            var finalPosition = chaoObject.transform.position;
            var distanceMoved = Vector3.Distance(initialPosition, finalPosition);
        
            Assert.Greater(distanceMoved, 0.01f, 
                "Chao should have moved in response to input");
            Assert.IsTrue(movementScript.isMoving || distanceMoved > 0.01f,
                "Movement script should register movement or Chao should have moved");
            
            Debug.Log($"[TEST] Movement_RespondsToInput test PASSED - Distance moved: {distanceMoved}");
        }

        [UnityTest]
        public IEnumerator EmoteBall_FollowsChao()
        {
            Debug.Log("[TEST] Starting EmoteBall_FollowsChao test");
        
            // Arrange
            var emoteBallScript = Object.FindAnyObjectByType<Chao_Pet.EmoteBallFollow>();
            Assert.IsNotNull(emoteBallScript, "EmoteBallFollow component should be present");
            Assert.IsNotNull(chaoObject, "Chao object not found");
        
            var initialChaoPosition = chaoObject.transform.position;
            var initialBallPosition = emoteBallScript.transform.position;
        
            // Act - Move the Chao
            chaoObject.transform.position = initialChaoPosition + Vector3.right * 2f;
        
            // Wait for EmoteBall to follow
            yield return new WaitForSeconds(1f);
        
            // Assert
            var finalBallPosition = emoteBallScript.transform.position;
            var distanceMoved = Vector3.Distance(initialBallPosition, finalBallPosition);
        
            Assert.Greater(distanceMoved, 0.1f, 
                "EmoteBall should follow when Chao moves");
            
            Debug.Log($"[TEST] EmoteBall_FollowsChao test PASSED - Ball moved: {distanceMoved}");
        }
    }
}
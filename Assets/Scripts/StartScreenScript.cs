using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transition from the start screen to the garden scene.
/// Responds to space key input to trigger the scene change.
/// </summary>
public class StartScreenScript : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "Garden";
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;
        
    [Header("Input Settings")]
    [SerializeField] private float inputCooldown = 0.5f; // Prevent rapid scene loading
        
    private bool isLoadingScene;
    private float lastInputTime;

    #region Unity Lifecycle

    private void Update()
    {
        if (isLoadingScene) return; // Prevent input during scene transition
            
        if (Input.GetKeyDown(triggerKey))
        {
            HandleSceneTransition();
        }
    }

    private void OnDestroy()
    {
        // Ensure any ongoing operations are cleaned up
        isLoadingScene = false;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles the scene transition with proper validation and state management.
    /// </summary>
    private void HandleSceneTransition()
    {
        // Input cooldown to prevent rapid triggering
        if (Time.time - lastInputTime < inputCooldown)
        {
            return;
        }
            
        lastInputTime = Time.time;
            
        // Validate scene exists before attempting to load
        if (!IsSceneValid(targetSceneName))
        {
            Debug.LogError($"[{nameof(StartScreenScript)}] Scene '{targetSceneName}' not found in build settings!");
            return;
        }
            
        // Prevent multiple scene loads
        if (isLoadingScene)
        {
            return;
        }
            
        LoadTargetScene();
    }

    /// <summary>
    /// Validates that the target scene exists in the build settings.
    /// </summary>
    /// <param name="sceneName">Name of the scene to validate</param>
    /// <returns>True if scene exists in build settings, false otherwise</returns>
    private bool IsSceneValid(string sceneName)
    {
        // Check if scene exists in build settings
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
            
        return false;
    }

    /// <summary>
    /// Loads the target scene with proper state management.
    /// </summary>
    private void LoadTargetScene()
    {
        isLoadingScene = true;
            
        Debug.Log($"[{nameof(StartScreenScript)}] Loading scene: {targetSceneName}");
            
        try
        {
            SceneManager.LoadScene(targetSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{nameof(StartScreenScript)}] Failed to load scene '{targetSceneName}': {e.Message}");
            isLoadingScene = false; // Reset state on error
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually triggers the scene transition.
    /// </summary>
    public void TriggerSceneTransition()
    {
        HandleSceneTransition();
    }

    /// <summary>
    /// Sets the target scene name.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    /// <summary>
    /// Sets the input cooldown time.
    /// </summary>
    /// <param name="cooldown">Cooldown time in seconds</param>
    public void SetInputCooldown(float cooldown)
    {
        inputCooldown = Mathf.Max(0f, cooldown);
    }

    #endregion
}
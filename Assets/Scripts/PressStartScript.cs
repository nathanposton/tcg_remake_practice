using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Controls the blinking visibility of a TilemapRenderer component.
/// Automatically toggles the renderer's enabled state at a specified interval.
/// </summary>
public class PressStartScript : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.0f;
    [SerializeField] private bool startBlinkingOnStart = true;
        
    private TilemapRenderer tilemapRenderer;
    private bool isBlinking;
    private bool isInitialized;

    #region Unity Lifecycle

    private void Start()
    {
        Initialize();
            
        if (startBlinkingOnStart)
        {
            StartBlinking();
        }
    }

    private void OnDestroy()
    {
        StopBlinking();
    }

    private void OnDisable()
    {
        StopBlinking();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the blinking animation if not already active.
    /// </summary>
    public void StartBlinking()
    {
        if (!isInitialized || isBlinking) return;
            
        isBlinking = true;
        InvokeRepeating(nameof(ToggleUI), 0, blinkSpeed);
    }

    /// <summary>
    /// Stops the blinking animation if currently active.
    /// </summary>
    public void StopBlinking()
    {
        if (!isBlinking) return;
            
        isBlinking = false;
        CancelInvoke(nameof(ToggleUI));
    }

    /// <summary>
    /// Sets the TilemapRenderer to visible and stops blinking.
    /// </summary>
    public void Show()
    {
        StopBlinking();
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Sets the TilemapRenderer to invisible and stops blinking.
    /// </summary>
    public void Hide()
    {
        StopBlinking();
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Sets the blink speed in seconds.
    /// </summary>
    /// <param name="speed">Time interval between toggles in seconds</param>
    public void SetBlinkSpeed(float speed)
    {
        blinkSpeed = Mathf.Max(0.1f, speed); // Prevent negative or zero values
            
        if (isBlinking)
        {
            // Restart blinking with new speed
            StopBlinking();
            StartBlinking();
        }
    }

    #endregion

    #region Private Methods

    private void Initialize()
    {
        // Get the TilemapRenderer component once at startup
        tilemapRenderer = GetComponent<TilemapRenderer>();
            
        // Safety check - make sure the component exists
        if (tilemapRenderer == null)
        {
            Debug.LogError($"[{nameof(PressStartScript)}] No TilemapRenderer component found on GameObject '{gameObject.name}'!");
            enabled = false; // Disable this component to prevent further errors
            return;
        }
            
        isInitialized = true;
    }

    private void ToggleUI()
    {
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = !tilemapRenderer.enabled;
        }
    }

    #endregion
}
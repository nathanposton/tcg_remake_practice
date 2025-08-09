using UnityEngine;

public class MakeComponentBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField, Tooltip("Interval between blinks in seconds")]
    private float blinkInterval = 1.0f;
        
    [SerializeField, Tooltip("Whether the blinking is currently active")]
    private bool isBlinking = true;
        
    [SerializeField, Tooltip("Whether to start blinking immediately when the component starts")]
    private bool startOnEnable = true;
        
    [Header("Component to Blink")]
    [SerializeField, Tooltip("The component to make blink. If null, will use this GameObject's Renderer")]
    private Component targetComponent;
        
    private Renderer targetRenderer;
    private float nextBlinkTime;
    private bool isVisible = true;
        
    private void Start()
    {
        // If no specific component is assigned, try to get the Renderer from this GameObject
        if (targetComponent == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogWarning($"MakeComponentBlink on {gameObject.name}: No target component assigned and no Renderer found on this GameObject.");
                return;
            }
        }
        else
        {
            // Try to get the Renderer component from the assigned target
            targetRenderer = targetComponent.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogWarning($"MakeComponentBlink on {gameObject.name}: Assigned target component does not have a Renderer component.");
                return;
            }
        }
            
        if (startOnEnable)
        {
            StartBlinking();
        }
    }
        
    private void Update()
    {
        if (!isBlinking || targetRenderer == null) return;
            
        if (Time.time >= nextBlinkTime)
        {
            ToggleVisibility();
            nextBlinkTime = Time.time + blinkInterval;
        }
    }
        
    private void ToggleVisibility()
    {
        isVisible = !isVisible;
        targetRenderer.enabled = isVisible;
    }
        
    /// <summary>
    /// Start the blinking effect
    /// </summary>
    public void StartBlinking()
    {
        isBlinking = true;
        nextBlinkTime = Time.time + blinkInterval;
    }
        
    /// <summary>
    /// Stop the blinking effect and ensure the component is visible
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;
        if (targetRenderer != null)
        {
            targetRenderer.enabled = true;
            isVisible = true;
        }
    }
        
    /// <summary>
    /// Set the blink interval in seconds
    /// </summary>
    /// <param name="interval">Interval in seconds</param>
    public void SetBlinkInterval(float interval)
    {
        blinkInterval = Mathf.Max(0.1f, interval); // Minimum 0.1 seconds to prevent too fast blinking
    }
        
    /// <summary>
    /// Get the current blink interval
    /// </summary>
    /// <returns>Current blink interval in seconds</returns>
    public float GetBlinkInterval()
    {
        return blinkInterval;
    }
        
    /// <summary>
    /// Check if the component is currently blinking
    /// </summary>
    /// <returns>True if blinking is active</returns>
    public bool IsBlinking()
    {
        return isBlinking;
    }
        
    private void OnEnable()
    {
        if (startOnEnable && targetRenderer != null)
        {
            StartBlinking();
        }
    }
        
    private void OnDisable()
    {
        // Ensure the component is visible when disabled
        if (targetRenderer != null)
        {
            targetRenderer.enabled = true;
            isVisible = true;
        }
    }
}
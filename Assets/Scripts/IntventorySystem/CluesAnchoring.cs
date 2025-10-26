using UnityEngine;

/// <summary>
/// Automatically anchors the clues panel to the top-right of the screen.
/// This ensures the panel maintains its position across different screen resolutions and aspect ratios.
/// Attach this script to the parent GameObject containing the clues UI elements.
/// </summary>
public class CluesAnchoring : MonoBehaviour
{
    [Header("Anchoring Settings")]
    [Tooltip("Distance from the right edge of the screen in pixels")]
    [SerializeField] private float rightOffset = 30f;

    [Tooltip("Distance from the top edge of the screen in pixels")]
    [SerializeField] private float topOffset = 30f;

    [Header("Auto-Configure on Start")]
    [Tooltip("Automatically configure anchoring when the scene loads")]
    [SerializeField] private bool autoConfigureOnAwake = true;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError($"CluesAnchoring: No RectTransform found on {gameObject.name}. This script must be attached to a UI GameObject.");
            return;
        }

        if (autoConfigureOnAwake)
        {
            ConfigureAnchoring();
        }
    }

    /// <summary>
    /// Configures the RectTransform to anchor the panel to the top-right of the screen.
    /// </summary>
    [ContextMenu("Configure Top-Right Anchoring")]
    public void ConfigureAnchoring()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError($"CluesAnchoring: No RectTransform found on {gameObject.name}");
            return;
        }

        // Set anchors to top-right
        // Anchor Min: (1, 1) - Right horizontal, top vertical
        // Anchor Max: (1, 1) - Right horizontal, top vertical
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);

        // Set pivot to top-right so the panel grows left and down from the top-right
        rectTransform.pivot = new Vector2(1f, 1f);

        // Position the panel at the specified offset from the top-right
        rectTransform.anchoredPosition = new Vector2(-rightOffset, -topOffset);

        Debug.Log($"CluesAnchoring: Configured {gameObject.name} to anchor at top-right with ({rightOffset}px, {topOffset}px) offset.");
    }

    /// <summary>
    /// Resets the panel to default position (useful for testing)
    /// </summary>
    [ContextMenu("Reset to Default Position")]
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(-rightOffset, -topOffset);
        }
    }

#if UNITY_EDITOR
    // Validate settings in the editor
    private void OnValidate()
    {
        if (rightOffset < 0)
        {
            rightOffset = 0;
            Debug.LogWarning("CluesAnchoring: Right offset cannot be negative. Reset to 0.");
        }

        if (topOffset < 0)
        {
            topOffset = 0;
            Debug.LogWarning("CluesAnchoring: Top offset cannot be negative. Reset to 0.");
        }
    }
#endif
}

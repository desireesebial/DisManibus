using UnityEngine;

/// <summary>
/// Automatically anchors the flashlight stamina UI to the bottom-left of the screen.
/// This ensures the UI maintains its position across different screen resolutions and aspect ratios.
/// Attach this script to the parent GameObject containing the flashlight stamina UI elements.
/// </summary>
public class FlashlightStaminaAnchoring : MonoBehaviour
{
    [Header("Anchoring Settings")]
    [Tooltip("Distance from the bottom edge of the screen in pixels")]
    [SerializeField] private float bottomOffset = 30f;

    [Tooltip("Distance from the left edge of the screen in pixels")]
    [SerializeField] private float leftOffset = 30f;

    [Header("Auto-Configure on Start")]
    [Tooltip("Automatically configure anchoring when the scene loads")]
    [SerializeField] private bool autoConfigureOnAwake = true;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError($"FlashlightStaminaAnchoring: No RectTransform found on {gameObject.name}. This script must be attached to a UI GameObject.");
            return;
        }

        if (autoConfigureOnAwake)
        {
            ConfigureAnchoring();
        }
    }

    /// <summary>
    /// Configures the RectTransform to anchor the UI to the bottom-left of the screen.
    /// </summary>
    [ContextMenu("Configure Bottom-Left Anchoring")]
    public void ConfigureAnchoring()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError($"FlashlightStaminaAnchoring: No RectTransform found on {gameObject.name}");
            return;
        }

        // Set anchors to bottom-left
        // Anchor Min: (0, 0) - Left horizontal, bottom vertical
        // Anchor Max: (0, 0) - Left horizontal, bottom vertical
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);

        // Set pivot to bottom-left so the UI grows right and upward from the bottom-left corner
        rectTransform.pivot = new Vector2(0f, 0f);

        // Position the UI at the specified offset from the bottom-left corner
        rectTransform.anchoredPosition = new Vector2(leftOffset, bottomOffset);

        Debug.Log($"FlashlightStaminaAnchoring: Configured {gameObject.name} to anchor at bottom-left with offsets ({leftOffset}px, {bottomOffset}px).");
    }

    /// <summary>
    /// Resets the UI to default position (useful for testing)
    /// </summary>
    [ContextMenu("Reset to Default Position")]
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(leftOffset, bottomOffset);
        }
    }

    /// <summary>
    /// Updates the offsets and repositions the UI
    /// </summary>
    /// <param name="newLeftOffset">New distance from left edge</param>
    /// <param name="newBottomOffset">New distance from bottom edge</param>
    public void SetOffsets(float newLeftOffset, float newBottomOffset)
    {
        leftOffset = newLeftOffset;
        bottomOffset = newBottomOffset;
        ResetPosition();
    }

#if UNITY_EDITOR
    // Validate settings in the editor
    private void OnValidate()
    {
        if (bottomOffset < 0)
        {
            bottomOffset = 0;
            Debug.LogWarning("FlashlightStaminaAnchoring: Bottom offset cannot be negative. Reset to 0.");
        }

        if (leftOffset < 0)
        {
            leftOffset = 0;
            Debug.LogWarning("FlashlightStaminaAnchoring: Left offset cannot be negative. Reset to 0.");
        }
    }
#endif
}

using UnityEngine;

/// <summary>
/// Automatically anchors the name/health/quest panel to the top-left of the screen.
/// This ensures the panel maintains its position across different screen resolutions and aspect ratios.
/// Attach this script to the parent GameObject containing the UI elements.
/// </summary>
public class NameHealthQuestAnchoring : MonoBehaviour
{
    [Header("Anchoring Settings")]
    [Tooltip("Distance from the left edge of the screen in pixels")]
    [SerializeField] private float leftOffset = 30f;

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
            Debug.LogError($"NameHealthQuestAnchoring: No RectTransform found on {gameObject.name}. This script must be attached to a UI GameObject.");
            return;
        }

        if (autoConfigureOnAwake)
        {
            ConfigureAnchoring();
        }
    }

    /// <summary>
    /// Configures the RectTransform to anchor the panel to the top-left of the screen.
    /// </summary>
    [ContextMenu("Configure Top-Left Anchoring")]
    public void ConfigureAnchoring()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError($"NameHealthQuestAnchoring: No RectTransform found on {gameObject.name}");
            return;
        }

        // Set anchors to top-left
        // Anchor Min: (0, 1) - Left horizontal, top vertical
        // Anchor Max: (0, 1) - Left horizontal, top vertical
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);

        // Set pivot to top-left so the panel grows right and down from the top-left
        rectTransform.pivot = new Vector2(0f, 1f);

        // Position the panel at the specified offset from the top-left
        rectTransform.anchoredPosition = new Vector2(leftOffset, -topOffset);

        Debug.Log($"NameHealthQuestAnchoring: Configured {gameObject.name} to anchor at top-left with ({leftOffset}px, {topOffset}px) offset.");
    }

    /// <summary>
    /// Resets the panel to default position (useful for testing)
    /// </summary>
    [ContextMenu("Reset to Default Position")]
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(leftOffset, -topOffset);
        }
    }

#if UNITY_EDITOR
    // Validate settings in the editor
    private void OnValidate()
    {
        if (leftOffset < 0)
        {
            leftOffset = 0;
            Debug.LogWarning("NameHealthQuestAnchoring: Left offset cannot be negative. Reset to 0.");
        }

        if (topOffset < 0)
        {
            topOffset = 0;
            Debug.LogWarning("NameHealthQuestAnchoring: Top offset cannot be negative. Reset to 0.");
        }
    }
#endif
}

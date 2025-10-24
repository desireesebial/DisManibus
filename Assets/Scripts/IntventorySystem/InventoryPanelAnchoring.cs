using UnityEngine;

/// <summary>
/// Automatically anchors the inventory panel to the bottom-center of the screen.
/// This ensures the panel maintains its position across different screen resolutions and aspect ratios.
/// Attach this script to the parent GameObject containing the inventory UI elements.
/// </summary>
public class InventoryPanelAnchoring : MonoBehaviour
{
    [Header("Anchoring Settings")]
    [Tooltip("Distance from the bottom edge of the screen in pixels")]
    [SerializeField] private float bottomOffset = 30f;

    [Tooltip("Distance from the left/right edges when panel reaches screen boundaries")]
    [SerializeField] private float horizontalPadding = 20f;

    [Header("Auto-Configure on Start")]
    [Tooltip("Automatically configure anchoring when the scene loads")]
    [SerializeField] private bool autoConfigureOnAwake = true;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError($"InventoryPanelAnchoring: No RectTransform found on {gameObject.name}. This script must be attached to a UI GameObject.");
            return;
        }

        if (autoConfigureOnAwake)
        {
            ConfigureAnchoring();
        }
    }

    /// <summary>
    /// Configures the RectTransform to anchor the panel to the bottom-center of the screen.
    /// </summary>
    [ContextMenu("Configure Bottom-Center Anchoring")]
    public void ConfigureAnchoring()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError($"InventoryPanelAnchoring: No RectTransform found on {gameObject.name}");
            return;
        }

        // Set anchors to bottom-center
        // Anchor Min: (0.5, 0) - Center horizontal, bottom vertical
        // Anchor Max: (0.5, 0) - Center horizontal, bottom vertical
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);

        // Set pivot to bottom-center so the panel grows upward from the bottom
        rectTransform.pivot = new Vector2(0.5f, 0f);

        // Position the panel at the specified offset from the bottom, centered horizontally
        rectTransform.anchoredPosition = new Vector2(0f, bottomOffset);

        Debug.Log($"InventoryPanelAnchoring: Configured {gameObject.name} to anchor at bottom-center with {bottomOffset}px offset.");
    }

    /// <summary>
    /// Resets the panel to default position (useful for testing)
    /// </summary>
    [ContextMenu("Reset to Default Position")]
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(0f, bottomOffset);
        }
    }

#if UNITY_EDITOR
    // Validate settings in the editor
    private void OnValidate()
    {
        if (bottomOffset < 0)
        {
            bottomOffset = 0;
            Debug.LogWarning("InventoryPanelAnchoring: Bottom offset cannot be negative. Reset to 0.");
        }
    }
#endif
}

using UnityEngine;

/// <summary>
/// Automatically anchors the dialogue panel just above the inventory panel at the bottom-center of the screen.
/// This ensures the dialogue maintains its position relative to the inventory across different screen resolutions and aspect ratios.
/// Attach this script to the parent GameObject containing the dialogue UI elements.
/// </summary>
public class DialoguePanelAnchoring : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("Reference to the inventory panel GameObject to position above")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Anchoring Settings")]
    [Tooltip("Vertical spacing between dialogue and inventory panel in pixels")]
    [SerializeField] private float spacingAboveInventory = 10f;

    [Header("Auto-Configure on Start")]
    [Tooltip("Automatically configure anchoring when the scene loads")]
    [SerializeField] private bool autoConfigureOnAwake = true;

    [Tooltip("Update position every frame (useful if inventory panel size changes)")]
    [SerializeField] private bool updateContinuously = false;

    private RectTransform rectTransform;
    private RectTransform inventoryRectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError($"DialoguePanelAnchoring: No RectTransform found on {gameObject.name}. This script must be attached to a UI GameObject.");
            return;
        }

        if (autoConfigureOnAwake)
        {
            ConfigureAnchoring();
        }
    }

    private void Update()
    {
        if (updateContinuously)
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// Configures the RectTransform to anchor the dialogue panel above the inventory panel at bottom-center.
    /// </summary>
    [ContextMenu("Configure Dialogue Anchoring")]
    public void ConfigureAnchoring()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError($"DialoguePanelAnchoring: No RectTransform found on {gameObject.name}");
            return;
        }

        // Find inventory panel if not assigned
        if (inventoryPanel == null)
        {
            // Try to find by name or tag
            inventoryPanel = GameObject.Find("InventoryPanel");
            if (inventoryPanel == null)
            {
                Debug.LogWarning($"DialoguePanelAnchoring: Inventory panel not assigned and could not be found automatically. Please assign it in the Inspector.");
            }
        }

        // Set anchors to bottom-center (same as inventory)
        // Anchor Min: (0.5, 0) - Center horizontal, bottom vertical
        // Anchor Max: (0.5, 0) - Center horizontal, bottom vertical
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);

        // Set pivot to bottom-center so the panel grows upward from the bottom
        rectTransform.pivot = new Vector2(0.5f, 0f);

        // Calculate position above inventory
        UpdatePosition();
    }

    /// <summary>
    /// Updates the position to be above the inventory panel
    /// </summary>
    private void UpdatePosition()
    {
        if (inventoryPanel == null || rectTransform == null)
            return;

        // Get inventory panel's RectTransform
        if (inventoryRectTransform == null)
        {
            inventoryRectTransform = inventoryPanel.GetComponent<RectTransform>();
        }

        if (inventoryRectTransform == null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Inventory panel does not have a RectTransform.");
            return;
        }

        // Calculate the inventory panel's bottom position + its height
        float inventoryBottomPosition = inventoryRectTransform.anchoredPosition.y;
        float inventoryHeight = inventoryRectTransform.rect.height;

        // Position dialogue above inventory: inventoryBottom + inventoryHeight + spacing
        float dialogueYPosition = inventoryBottomPosition + inventoryHeight + spacingAboveInventory;

        // Set the dialogue position
        rectTransform.anchoredPosition = new Vector2(0f, dialogueYPosition);

        Debug.Log($"DialoguePanelAnchoring: Positioned {gameObject.name} at Y={dialogueYPosition} (Inventory bottom: {inventoryBottomPosition}, height: {inventoryHeight}, spacing: {spacingAboveInventory})");
    }

    /// <summary>
    /// Manually trigger a position update (useful after inventory panel size changes)
    /// </summary>
    [ContextMenu("Update Position")]
    public void RefreshPosition()
    {
        UpdatePosition();
    }

#if UNITY_EDITOR
    // Validate settings in the editor
    private void OnValidate()
    {
        if (spacingAboveInventory < 0)
        {
            spacingAboveInventory = 0;
            Debug.LogWarning("DialoguePanelAnchoring: Spacing cannot be negative. Reset to 0.");
        }
    }
#endif
}

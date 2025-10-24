using UnityEngine;
using UnityEngine.UI;

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

    [Tooltip("Disable automatic positioning (for manual control/testing)")]
    [SerializeField] private bool disableAutomaticPositioning = false;

    [Tooltip("Update position every frame (useful if inventory panel size changes)")]
    [SerializeField] private bool updateContinuously = false;

    [Header("Debug")]
    [Tooltip("Show detailed position calculation logs in Console")]
    [SerializeField] private bool showDebugLogs = true;

    [Header("Layout Component Handling")]
    [Tooltip("Automatically remove/disable layout components that conflict with manual positioning")]
    [SerializeField] private bool removeConflictingLayoutComponents = true;

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

    private void Start()
    {
        // Call ConfigureAnchoring again in Start() to ensure anchors are set
        // This is a backup in case Awake() didn't work or anchors were reset
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

        // Remove conflicting layout components that drive the RectTransform
        if (removeConflictingLayoutComponents)
        {
            RemoveConflictingLayoutComponents();
        }

        // Log current anchor values before changing
        if (showDebugLogs)
        {
            Debug.Log($"[DialoguePanelAnchoring] BEFORE setting anchors on {gameObject.name}:");
            Debug.Log($"[DialoguePanelAnchoring]   AnchorMin: {rectTransform.anchorMin}");
            Debug.Log($"[DialoguePanelAnchoring]   AnchorMax: {rectTransform.anchorMax}");
            Debug.Log($"[DialoguePanelAnchoring]   Pivot: {rectTransform.pivot}");
        }

        // ALWAYS set anchors to bottom-center (same as inventory)
        // This ensures dialogue and inventory move together when screen resizes
        // Anchor Min: (0.5, 0) - Center horizontal, bottom vertical
        // Anchor Max: (0.5, 0) - Center horizontal, bottom vertical
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);

        // Set pivot to bottom-center so the panel grows upward from the bottom
        rectTransform.pivot = new Vector2(0.5f, 0f);

        // Log after setting to confirm changes
        if (showDebugLogs)
        {
            Debug.Log($"[DialoguePanelAnchoring] AFTER setting anchors on {gameObject.name}:");
            Debug.Log($"[DialoguePanelAnchoring]   AnchorMin: {rectTransform.anchorMin}");
            Debug.Log($"[DialoguePanelAnchoring]   AnchorMax: {rectTransform.anchorMax}");
            Debug.Log($"[DialoguePanelAnchoring]   Pivot: {rectTransform.pivot}");
        }

        // Only calculate and update position if automatic positioning is enabled
        if (disableAutomaticPositioning)
        {
            if (showDebugLogs)
                Debug.Log($"[DialoguePanelAnchoring] Automatic positioning is disabled for {gameObject.name}. Anchors are set, but position will remain as manually configured.");
            return;
        }

        // Calculate position above inventory
        UpdatePosition();
    }

    /// <summary>
    /// Updates the position to be above the inventory panel
    /// </summary>
    private void UpdatePosition()
    {
        if (disableAutomaticPositioning)
            return;

        if (inventoryPanel == null || rectTransform == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"DialoguePanelAnchoring: Cannot update position - inventoryPanel or rectTransform is null on {gameObject.name}");
            return;
        }

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
        float inventoryPivotY = inventoryRectTransform.pivot.y;
        Vector2 inventoryAnchorMin = inventoryRectTransform.anchorMin;
        Vector2 inventoryAnchorMax = inventoryRectTransform.anchorMax;

        // Position dialogue above inventory: inventoryBottom + inventoryHeight + spacing
        float dialogueYPosition = inventoryBottomPosition + inventoryHeight + spacingAboveInventory;

        // Debug logging
        if (showDebugLogs)
        {
            Debug.Log($"[DialoguePanelAnchoring] === Position Calculation Debug ===");
            Debug.Log($"[DialoguePanelAnchoring] Dialogue GameObject: {gameObject.name}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory Panel: {inventoryPanel.name}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory AnchoredPosition.y: {inventoryBottomPosition}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory Height: {inventoryHeight}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory Pivot.y: {inventoryPivotY}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory Anchor Min: {inventoryAnchorMin}");
            Debug.Log($"[DialoguePanelAnchoring] Inventory Anchor Max: {inventoryAnchorMax}");
            Debug.Log($"[DialoguePanelAnchoring] Spacing Above Inventory: {spacingAboveInventory}");
            Debug.Log($"[DialoguePanelAnchoring] Calculated Dialogue Y Position: {dialogueYPosition}");
            Debug.Log($"[DialoguePanelAnchoring] Dialogue Height: {rectTransform.rect.height}");
            Debug.Log($"[DialoguePanelAnchoring] Dialogue Pivot: {rectTransform.pivot}");
            Debug.Log($"[DialoguePanelAnchoring] Dialogue Anchor Min: {rectTransform.anchorMin}");
            Debug.Log($"[DialoguePanelAnchoring] Dialogue Anchor Max: {rectTransform.anchorMax}");
        }

        // Set the dialogue position
        rectTransform.anchoredPosition = new Vector2(0f, dialogueYPosition);

        if (showDebugLogs)
        {
            Debug.Log($"[DialoguePanelAnchoring] Final Dialogue AnchoredPosition: {rectTransform.anchoredPosition}");
            Debug.Log($"[DialoguePanelAnchoring] === End Position Calculation ===");
        }
    }

    /// <summary>
    /// Manually trigger a position update (useful after inventory panel size changes)
    /// </summary>
    [ContextMenu("Update Position")]
    public void RefreshPosition()
    {
        UpdatePosition();
    }

    /// <summary>
    /// Removes layout components that prevent manual RectTransform positioning
    /// </summary>
    private void RemoveConflictingLayoutComponents()
    {
        bool removedAny = false;

        // Check for and remove ContentSizeFitter
        ContentSizeFitter contentSizeFitter = GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing ContentSizeFitter from {gameObject.name} to allow manual positioning.");
            if (Application.isPlaying)
                Destroy(contentSizeFitter);
            else
                DestroyImmediate(contentSizeFitter);
            removedAny = true;
        }

        // Check for and remove LayoutElement
        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null && layoutElement.enabled)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Disabling LayoutElement on {gameObject.name} to allow manual positioning.");
            layoutElement.enabled = false;
            removedAny = true;
        }

        // Check for and remove VerticalLayoutGroup
        VerticalLayoutGroup verticalLayout = GetComponent<VerticalLayoutGroup>();
        if (verticalLayout != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing VerticalLayoutGroup from {gameObject.name} to allow manual positioning.");
            if (Application.isPlaying)
                Destroy(verticalLayout);
            else
                DestroyImmediate(verticalLayout);
            removedAny = true;
        }

        // Check for and remove HorizontalLayoutGroup
        HorizontalLayoutGroup horizontalLayout = GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing HorizontalLayoutGroup from {gameObject.name} to allow manual positioning.");
            if (Application.isPlaying)
                Destroy(horizontalLayout);
            else
                DestroyImmediate(horizontalLayout);
            removedAny = true;
        }

        // Check for and remove GridLayoutGroup
        GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing GridLayoutGroup from {gameObject.name} to allow manual positioning.");
            if (Application.isPlaying)
                Destroy(gridLayout);
            else
                DestroyImmediate(gridLayout);
            removedAny = true;
        }

        // Check for and remove Canvas (this is the main cause of "driven by Canvas")
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing Canvas component from {gameObject.name} to allow manual positioning. Make sure there's a parent Canvas in the scene.");
            if (Application.isPlaying)
                Destroy(canvas);
            else
                DestroyImmediate(canvas);
            removedAny = true;
        }

        // Check for and remove GraphicRaycaster (usually comes with Canvas)
        UnityEngine.UI.GraphicRaycaster raycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing GraphicRaycaster from {gameObject.name}.");
            if (Application.isPlaying)
                Destroy(raycaster);
            else
                DestroyImmediate(raycaster);
            removedAny = true;
        }

        // Check for and remove CanvasScaler (usually comes with Canvas)
        UnityEngine.UI.CanvasScaler scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            Debug.LogWarning($"DialoguePanelAnchoring: Removing CanvasScaler from {gameObject.name}.");
            if (Application.isPlaying)
                Destroy(scaler);
            else
                DestroyImmediate(scaler);
            removedAny = true;
        }

        if (removedAny)
        {
            Debug.Log($"DialoguePanelAnchoring: Successfully removed conflicting layout components from {gameObject.name}. Manual positioning is now enabled.");
        }
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

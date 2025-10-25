using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Allows the player to interact with an object to complete and/or start quests.
/// Uses the simple QuestManager system (string-based).
/// </summary>
public class ItemQuestInteract : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("Quest ID to complete when player interacts (leave empty to skip)")]
    public string questToComplete = "";

    [Tooltip("Quest ID to start when player interacts (leave empty to skip)")]
    public string questToStartID = "";

    [Tooltip("Quest text/description for the new quest (used with questToStartID)")]
    [TextArea]
    public string questToStartText = "";

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to interact";

    [Header("Crosshair Detection")]
    public bool useCrosshairDetection = true;
    public float crosshairDetectionRange = 10f;
    public LayerMask objectLayerMask = -1;

    [Header("Crosshair Feedback")]
    public bool changeCrosshairColor = true;
    public Color crosshairHoverColor = Color.yellow;

    [Header("UI References")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionTextUI;

    [Header("Behavior Options")]
    [Tooltip("If true, can only be interacted with once. If false, can be used multiple times")]
    public bool interactOnce = true;

    [Tooltip("If true, disable the GameObject after interaction")]
    public bool disableAfterInteraction = false;

    [Header("Optional References")]
    [Tooltip("Optional: Reference to FirstPersonController. If not set, will find it automatically")]
    public FirstPersonController playerController;

    // Internal state
    private bool playerInRange = false;
    private bool hasInteracted = false;

    // Player references
    private Camera playerCamera;
    private Image crosshairObject;
    private Color originalCrosshairColor;
    private bool wasHovering = false;

    // Cached collider reference
    private Collider objectCollider;

    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            playerCamera = player.GetComponentInChildren<Camera>();

            // Get crosshair reference from FirstPersonController
            if (playerController != null)
            {
                crosshairObject = playerController.GetComponentInChildren<Image>();
                if (crosshairObject != null)
                {
                    originalCrosshairColor = crosshairObject.color;
                }
            }
        }

        // Collider Validation
        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            objectCollider = GetComponentInChildren<Collider>();
        }

        if (useCrosshairDetection && objectCollider == null)
        {
            Debug.LogError($"[ItemQuestInteract] {gameObject.name}: Crosshair detection is enabled but NO COLLIDER found! Please add a collider to this GameObject.");
        }
        else if (objectCollider != null)
        {
            // Check if collider's layer matches the layer mask
            int colliderLayer = objectCollider.gameObject.layer;
            bool layerMatches = (objectLayerMask == -1) || ((objectLayerMask.value & (1 << colliderLayer)) != 0);

            if (!layerMatches)
            {
                Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: Collider is on layer '{LayerMask.LayerToName(colliderLayer)}' which doesn't match objectLayerMask! Raycast detection may fail.");
            }
        }

        // UI Validation
        if (interactionUI == null)
        {
            Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: interactionUI not assigned! Interaction prompt won't show.");
        }
        else
        {
            // Try to auto-find TextMeshProUGUI if not assigned
            if (interactionTextUI == null)
            {
                interactionTextUI = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
            }

            interactionUI.SetActive(false);
        }

        // Validate quest settings
        if (string.IsNullOrEmpty(questToComplete) && string.IsNullOrEmpty(questToStartID))
        {
            Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: No quests configured! Set questToComplete and/or questToStartID.");
        }

        if (!string.IsNullOrEmpty(questToStartID) && string.IsNullOrEmpty(questToStartText))
        {
            Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: questToStartID is set but questToStartText is empty!");
        }
    }

    void Update()
    {
        CheckPlayerDistance();
        HandleInteraction();
    }

    void CheckPlayerDistance()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = playerInRange;

        if (useCrosshairDetection)
        {
            playerInRange = IsObjectUnderCrosshair() && distance <= crosshairDetectionRange;
        }
        else
        {
            playerInRange = distance <= interactionRange;
        }

        // Show/hide UI and crosshair feedback
        if (playerInRange && !wasInRange)
        {
            // Check if already interacted and interactOnce is true
            if (interactOnce && hasInteracted)
            {
                return; // Don't show prompt if already interacted
            }

            ShowInteractionUI();
            ChangeCrosshairColor(true);
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
            ChangeCrosshairColor(false);
        }
    }

    void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            // Check if already interacted
            if (interactOnce && hasInteracted)
            {
                Debug.Log($"[ItemQuestInteract] {gameObject.name}: Already interacted (interactOnce is enabled)");
                return;
            }

            TriggerQuestEvents();
        }
    }

    bool IsObjectUnderCrosshair()
    {
        if (playerCamera == null) return false;

        // Cast ray from center of screen (crosshair position)
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, objectLayerMask))
        {
            // Check if the hit object is this object or its child
            return hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);
        }

        return false;
    }

    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null)
            {
                interactionTextUI.text = interactionText;
            }
        }
    }

    void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void ChangeCrosshairColor(bool isHovering)
    {
        if (!changeCrosshairColor || crosshairObject == null) return;

        if (isHovering && !wasHovering)
        {
            crosshairObject.color = crosshairHoverColor;
            wasHovering = true;
        }
        else if (!isHovering && wasHovering)
        {
            crosshairObject.color = originalCrosshairColor;
            wasHovering = false;
        }
    }

    void TriggerQuestEvents()
    {
        Debug.Log($"[ItemQuestInteract] {gameObject.name}: Player interacted!");

        hasInteracted = true;

        // Hide UI
        HideInteractionUI();
        ChangeCrosshairColor(false);

        // Complete quest if specified
        if (!string.IsNullOrEmpty(questToComplete))
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteQuest(questToComplete);
                Debug.Log($"[ItemQuestInteract] {gameObject.name}: Completed quest '{questToComplete}'");
            }
            else
            {
                Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: QuestManager.Instance is null! Cannot complete quest.");
            }
        }

        // Start new quest if specified
        if (!string.IsNullOrEmpty(questToStartID) && !string.IsNullOrEmpty(questToStartText))
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddQuest(questToStartID, questToStartText);
                Debug.Log($"[ItemQuestInteract] {gameObject.name}: Started quest '{questToStartID}'");
            }
            else
            {
                Debug.LogWarning($"[ItemQuestInteract] {gameObject.name}: QuestManager.Instance is null! Cannot start quest.");
            }
        }

        // Disable GameObject if requested
        if (disableAfterInteraction)
        {
            Debug.Log($"[ItemQuestInteract] {gameObject.name}: Disabling GameObject after interaction");
            gameObject.SetActive(false);
        }
    }

    // Public method to reset (useful for testing or special cases)
    public void ResetInteractable()
    {
        hasInteracted = false;
        Debug.Log($"[ItemQuestInteract] {gameObject.name}: Interactable reset");
    }

    // Visual debugging in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw interaction range sphere (simple distance detection)
        if (!useCrosshairDetection)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        // Draw crosshair detection range sphere
        if (useCrosshairDetection)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, crosshairDetectionRange);
        }

        // Draw collider bounds if available
        if (objectCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(objectCollider.bounds.center, objectCollider.bounds.size);
        }
        else
        {
            // Warning: No collider found
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}

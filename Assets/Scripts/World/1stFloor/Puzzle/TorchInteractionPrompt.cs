using UnityEngine;
using TMPro;

/// <summary>
/// Reusable interaction prompt system for SequentialTorch objects
/// Shows TextMeshPro prompt at FIXED position on player's camera screen (2D overlay)
/// Text appears at fixed screen location (e.g., bottom-center) - does NOT track torch position
/// Requires: ScreenSpace Canvas and TextMeshProUGUI assigned in Inspector
/// IMPORTANT: Canvas must be ScreenSpaceOverlay or ScreenSpaceCamera (NOT WorldSpace)
/// </summary>
public class TorchInteractionPrompt : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The torch this prompt is for (auto-assigned if on same GameObject)")]
    [SerializeField] private SequentialTorch torch;

    [Header("UI References - SCREEN SPACE ONLY")]
    [Tooltip("Canvas to use (MUST be ScreenSpaceOverlay or ScreenSpaceCamera, NOT WorldSpace)")]
    [SerializeField] private Canvas promptCanvas;

    [Tooltip("TextMeshPro text component to use (child of ScreenSpace canvas)")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Tooltip("CanvasGroup for controlling visibility (optional - will be auto-added if missing)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Prompt Settings")]
    [Tooltip("Text to display in the prompt")]
    [SerializeField] private string displayText = "Press F to light torch";

    [Tooltip("How close player needs to be to see prompt")]
    [SerializeField] private float interactionDistance = 3f;

    [Tooltip("Max angle between player look direction and torch (degrees)")]
    [SerializeField] private float lookAngleThreshold = 45f;


    // Private references
    private Transform player;
    private Camera playerCamera;
    private bool showingPrompt = false;

    private void Awake()
    {
        // Auto-find torch if not assigned
        if (torch == null)
        {
            torch = GetComponent<SequentialTorch>();
            if (torch == null)
            {
                Debug.LogError("[TorchInteractionPrompt] No SequentialTorch found on GameObject!");
            }
        }
    }

    private void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            player = playerObj.transform;

            // Get player camera
            playerCamera = playerObj.GetComponentInChildren<Camera>();
            if (!playerCamera)
            {
                playerCamera = Camera.main;
            }
        }

        if (!playerCamera)
        {
            Debug.LogError("[TorchInteractionPrompt] No camera found!");
        }

        // Validate UI references
        if (promptCanvas == null)
        {
            Debug.LogError("[TorchInteractionPrompt] No Canvas assigned! Please assign a ScreenSpace Canvas in the Inspector.");
            return;
        }

        if (promptText == null)
        {
            Debug.LogError("[TorchInteractionPrompt] No TextMeshProUGUI assigned! Please assign a TextMeshProUGUI GameObject in the Inspector.");
            return;
        }

        // Validate canvas is ScreenSpace only (reject WorldSpace)
        if (promptCanvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogError("[TorchInteractionPrompt] Canvas must be ScreenSpaceOverlay or ScreenSpaceCamera! WorldSpace is NOT supported. Current mode: WorldSpace. Please change Canvas RenderMode in Inspector.");
            enabled = false; // Disable this component
            return;
        }

        Debug.Log($"[TorchInteractionPrompt] ✓ Canvas validated: {promptCanvas.renderMode} (screen overlay mode)");

        // Get or add CanvasGroup for visibility control
        if (canvasGroup == null)
        {
            canvasGroup = promptText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptText.gameObject.AddComponent<CanvasGroup>();
                Debug.Log($"[TorchInteractionPrompt] CanvasGroup auto-added to TMP GameObject");
            }
        }

        // Set initial text
        promptText.text = displayText;

        // Note: Anchor position is set manually in Inspector, not via script
        Debug.Log($"[TorchInteractionPrompt] Text anchor position will be controlled via Inspector");

        // Start completely invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            Debug.Log($"[TorchInteractionPrompt] CanvasGroup alpha set to 0 (invisible)");
        }
        SetPromptVisible(false);

        Debug.Log($"[TorchInteractionPrompt] Initialized (Camera: {playerCamera != null}, Canvas: {promptCanvas != null}, TMP: {promptText != null}, CanvasGroup: {canvasGroup != null})");
    }

    private void Update()
    {
        if (!player || !playerCamera || !torch || promptCanvas == null || promptText == null)
        {
            Debug.LogWarning($"[TorchInteractionPrompt] Missing references - hiding prompt");
            SetPromptVisible(false);
            return;
        }

        // Don't show prompt if torch is already lit
        if (torch.IsLit)
        {
            if (showingPrompt)
            {
                Debug.Log($"[TorchInteractionPrompt] Torch {torch.SequenceNumber} is lit - hiding prompt");
            }
            SetPromptVisible(false);
            return;
        }

        // Check if player can interact and show/hide prompt
        // Text stays at fixed position on screen - doesn't track torch
        bool canInteract = IsPlayerLookingAtTorch();

        // Only log when state changes to reduce spam
        if (canInteract != showingPrompt)
        {
            Debug.Log($"[TorchInteractionPrompt] Torch {torch.SequenceNumber} - Player canInteract: {canInteract}, Distance: {Vector3.Distance(transform.position, player.position):F2}m");
        }

        SetPromptVisible(canInteract);
    }

    private bool IsPlayerLookingAtTorch()
    {
        // Check distance first
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > interactionDistance)
        {
            return false;
        }

        // Check look angle
        Vector3 directionToTorch = (transform.position - playerCamera.transform.position).normalized;
        Vector3 cameraForward = playerCamera.transform.forward;
        float angle = Vector3.Angle(cameraForward, directionToTorch);

        if (angle > lookAngleThreshold)
        {
            return false;
        }

        // Raycast for line of sight
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToTorch, out hit, distance + 0.5f))
        {
            // Check if we hit this torch or its parent
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }

    private void SetPromptVisible(bool visible)
    {
        if (showingPrompt == visible) return;

        showingPrompt = visible;

        // Use CanvasGroup alpha for smooth visibility control
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            Debug.Log($"[TorchInteractionPrompt] Torch {torch?.SequenceNumber} - Visibility set to: {visible} (alpha: {canvasGroup.alpha})");
        }
        else
        {
            // Fallback to SetActive if CanvasGroup not available
            if (promptText != null)
            {
                promptText.gameObject.SetActive(visible);
                Debug.Log($"[TorchInteractionPrompt] Torch {torch?.SequenceNumber} - Visibility set to: {visible} (using SetActive fallback)");
            }
        }
    }

    // Public methods for customization
    public void SetPromptText(string text)
    {
        displayText = text;
        if (promptText != null)
        {
            promptText.text = text;
        }
    }

    public void SetInteractionDistance(float distance)
    {
        interactionDistance = distance;
    }

    public void SetLookAngleThreshold(float angle)
    {
        lookAngleThreshold = angle;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw look angle cone (approximate)
        Gizmos.color = Color.yellow;
        if (Application.isPlaying && playerCamera != null)
        {
            Vector3 toTorch = (transform.position - playerCamera.transform.position).normalized;
            Gizmos.DrawRay(playerCamera.transform.position, toTorch * interactionDistance);
        }
    }
}

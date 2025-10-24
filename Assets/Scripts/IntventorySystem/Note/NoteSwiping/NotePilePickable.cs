using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotePilePickable : MonoBehaviour
{
    [Header("References")]
    public NotePileSwiper pileSwiper;
    public Canvas pileCanvas; // Optional: UI canvas holding the pile pages

    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to sift papers";

    [Header("Crosshair Detection")]
    public bool useCrosshairDetection = true;
    public float crosshairDetectionRange = 10f;
    public LayerMask pileLayerMask = -1;

    [Header("UI Prompt")] 
    public GameObject interactionUI;
    public TextMeshProUGUI interactionTextUI;

    [Header("On Cleared")]
	public bool hideWorldPileOnCleared = true;
	public GameObject worldPileObject; // Defaults to this GameObject if null

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showDebugRays = true;

    private bool playerInRange = false;
    private bool isActiveSession = false;

    // Player refs
    private FirstPersonController firstPersonController;
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    private Image crosshairObject;
    private Color originalCrosshairColor;
    public bool changeCrosshairColor = true;
    public Color crosshairHoverColor = Color.yellow;

    // Cached collider reference
    private Collider pileCollider;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            firstPersonController = player.GetComponent<FirstPersonController>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerCamera = player.GetComponentInChildren<Camera>();
            if (firstPersonController != null)
            {
                crosshairObject = firstPersonController.GetComponentInChildren<Image>();
                if (crosshairObject != null) originalCrosshairColor = crosshairObject.color;
            }
        }

        // Collider Validation
        pileCollider = GetComponent<Collider>();
        if (pileCollider == null)
        {
            // Check children
            pileCollider = GetComponentInChildren<Collider>();
        }

        if (useCrosshairDetection && pileCollider == null)
        {
            Debug.LogError($"[NotePilePickable] {gameObject.name}: Crosshair detection is enabled but NO COLLIDER found! Please add a collider to this GameObject or its children.");
            Debug.LogError($"[NotePilePickable] {gameObject.name}: Add a Box Collider, Sphere Collider, or Mesh Collider to enable raycast detection.");
        }
        else if (pileCollider != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[NotePilePickable] {gameObject.name}: Found collider: {pileCollider.GetType().Name} on '{pileCollider.gameObject.name}'");
                Debug.Log($"[NotePilePickable] {gameObject.name}: Collider layer: {LayerMask.LayerToName(pileCollider.gameObject.layer)} (Layer {pileCollider.gameObject.layer})");
            }

            // Check if collider's layer matches the layer mask
            int colliderLayer = pileCollider.gameObject.layer;
            bool layerMatches = (pileLayerMask == -1) || ((pileLayerMask.value & (1 << colliderLayer)) != 0);

            if (!layerMatches)
            {
                Debug.LogWarning($"[NotePilePickable] {gameObject.name}: Collider is on layer '{LayerMask.LayerToName(colliderLayer)}' which doesn't match pileLayerMask! Raycast detection may fail.");
                Debug.LogWarning($"[NotePilePickable] {gameObject.name}: Either change the GameObject's layer or adjust pileLayerMask in Inspector.");
            }
        }

        // UI Validation and Auto-Setup
        if (interactionUI == null)
        {
            Debug.LogError($"[NotePilePickable] {gameObject.name}: interactionUI not assigned in Inspector! Text prompt won't show. Please assign a UI GameObject.");
            Debug.LogError($"[NotePilePickable] {gameObject.name}: Create a UI Canvas with TextMeshProUGUI and assign it to the interactionUI field.");
        }
        else
        {
            // Try to auto-find TextMeshProUGUI if not assigned
            if (interactionTextUI == null)
            {
                interactionTextUI = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
                if (interactionTextUI != null)
                {
                    if (enableDebugLogs) Debug.Log($"[NotePilePickable] {gameObject.name}: Auto-found TextMeshProUGUI component: {interactionTextUI.name}");
                }
                else
                {
                    Debug.LogError($"[NotePilePickable] {gameObject.name}: interactionTextUI not assigned and couldn't auto-find TextMeshProUGUI! Text won't update. Please assign a TextMeshProUGUI component.");
                }
            }

            interactionUI.SetActive(false);
        }

        if (pileSwiper != null)
        {
            pileSwiper.OnPileCleared -= HandlePileCleared;
            pileSwiper.OnPileCleared += HandlePileCleared;
            pileSwiper.OnCancelled -= HandleCancelled;
            pileSwiper.OnCancelled += HandleCancelled;
        }
        else
        {
            Debug.LogError($"[NotePilePickable] {gameObject.name}: pileSwiper not assigned! Note pile won't work.");
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
            bool underCrosshair = IsPileUnderCrosshair();
            bool withinRange = distance <= crosshairDetectionRange;
            playerInRange = underCrosshair && withinRange;

            if (enableDebugLogs && (playerInRange != wasInRange))
            {
                if (playerInRange)
                {
                    Debug.Log($"[NotePilePickable] {gameObject.name}: Player NOW IN RANGE (Crosshair detection)");
                }
                else
                {
                    Debug.Log($"[NotePilePickable] {gameObject.name}: Player out of range. UnderCrosshair: {underCrosshair}, WithinDistance: {withinRange} ({distance:F2}m / {crosshairDetectionRange}m)");
                }
            }
        }
        else
        {
            playerInRange = distance <= interactionRange;

            if (enableDebugLogs && (playerInRange != wasInRange))
            {
                if (playerInRange)
                {
                    Debug.Log($"[NotePilePickable] {gameObject.name}: Player NOW IN RANGE (Distance: {distance:F2}m / {interactionRange}m)");
                }
                else
                {
                    Debug.Log($"[NotePilePickable] {gameObject.name}: Player out of range (Distance: {distance:F2}m / {interactionRange}m)");
                }
            }
        }

        if (playerInRange && !wasInRange && !isActiveSession)
        {
            if (enableDebugLogs) Debug.Log($"[NotePilePickable] {gameObject.name}: Attempting to show interaction UI...");
            ShowInteractionUI();
            ChangeCrosshairColor(true);
        }
        else if ((!playerInRange || isActiveSession) && wasInRange)
        {
            if (enableDebugLogs) Debug.Log($"[NotePilePickable] {gameObject.name}: Hiding interaction UI. PlayerInRange: {playerInRange}, ActiveSession: {isActiveSession}");
            HideInteractionUI();
            ChangeCrosshairColor(false);
        }
    }

    void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (!isActiveSession)
            {
                BeginSwipingSession();
            }
        }
    }

    void BeginSwipingSession()
    {
        if (pileSwiper == null)
        {
            Debug.LogError("NotePileSwiper not assigned on NotePilePickable");
            return;
        }

        // If cleared, automatically reset the pile to start over
        if ((pileSwiper.keepLastPage && pileSwiper.IsOnlyLastPageRemaining()) || pileSwiper.IsCleared())
        {
            pileSwiper.ResetPileToStart();
            
            // Re-enable the world pile object if it was hidden
            if (hideWorldPileOnCleared)
            {
                GameObject toShow = worldPileObject != null ? worldPileObject : gameObject;
                if (toShow != null) toShow.SetActive(true);
            }
        }

        isActiveSession = true;
        HideInteractionUI();
        ChangeCrosshairColor(false);

        DisablePlayerControls();

        if (pileCanvas != null) pileCanvas.gameObject.SetActive(true);
        pileSwiper.ActivatePile(true);
    }

    void HandlePileCleared()
    {
		// Automatically close the canvas and restore controls after all papers are swiped
		RestoreSession();
        
		// Optionally hide the world pile object immediately after all pages are swiped
		if (hideWorldPileOnCleared)
		{
			GameObject toHide = worldPileObject != null ? worldPileObject : gameObject;
			if (toHide != null) toHide.SetActive(false);
		}

        // Show final note if provided (after controls are restored, so player can read it)
        if (pileSwiper != null && pileSwiper.finalNote != null && pileSwiper.noteUI != null)
        {
            pileSwiper.noteUI.ShowNote(pileSwiper.finalNote);
        }
    }

    void HandleCancelled()
    {
        RestoreSession();
    }

    void RestoreAfterNote()
    {
        if (pileSwiper != null && pileSwiper.noteUI != null)
        {
            pileSwiper.noteUI.OnNoteClosed -= RestoreAfterNote;
        }
        RestoreSession();
    }

    void RestoreSession()
    {
        isActiveSession = false;
        if (pileCanvas != null) pileCanvas.gameObject.SetActive(false);
        pileSwiper.ActivatePile(false);
        EnablePlayerControls();
    }

    bool IsPileUnderCrosshair()
    {
        if (playerCamera == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"[NotePilePickable] {gameObject.name}: playerCamera is null! Cannot perform raycast.");
            return false;
        }

        // Cast ray from center of screen (crosshair position)
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Draw debug ray if enabled
        if (showDebugRays)
        {
            Debug.DrawRay(ray.origin, ray.direction * crosshairDetectionRange, Color.cyan, 0.1f);
        }

        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, pileLayerMask))
        {
            // Draw hit point
            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 0.1f);
            }

            bool isThisPile = hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);

            if (enableDebugLogs && isThisPile)
            {
                Debug.Log($"[NotePilePickable] {gameObject.name}: Raycast HIT this pile! Collider: '{hit.collider.name}' at distance {hit.distance:F2}m");
            }

            return isThisPile;
        }
        else
        {
            // No hit - draw red ray
            if (showDebugRays)
            {
                Debug.DrawRay(ray.origin, ray.direction * crosshairDetectionRange, Color.red, 0.1f);
            }
        }

        return false;
    }

    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);

            if (enableDebugLogs)
            {
                Debug.Log($"[NotePilePickable] {gameObject.name}: ✓ Showing interaction UI");
                Debug.Log($"[NotePilePickable] {gameObject.name}: UI GameObject: '{interactionUI.name}', Active: {interactionUI.activeSelf}, ActiveInHierarchy: {interactionUI.activeInHierarchy}");
            }

            if (interactionTextUI != null)
            {
                interactionTextUI.text = interactionText;

                if (enableDebugLogs)
                {
                    Debug.Log($"[NotePilePickable] {gameObject.name}: ✓ Set prompt text to: \"{interactionText}\"");
                    Debug.Log($"[NotePilePickable] {gameObject.name}: TextMeshProUGUI enabled: {interactionTextUI.enabled}, GameObject active: {interactionTextUI.gameObject.activeInHierarchy}");
                }
            }
            else
            {
                Debug.LogError($"[NotePilePickable] {gameObject.name}: ✗ interactionTextUI is NULL! Text won't display!");
                Debug.LogError($"[NotePilePickable] {gameObject.name}: Please assign a TextMeshProUGUI component in Inspector.");
            }
        }
        else
        {
            Debug.LogError($"[NotePilePickable] {gameObject.name}: ✗ Cannot show UI - interactionUI is NULL!");
            Debug.LogError($"[NotePilePickable] {gameObject.name}: Please assign the interaction UI GameObject in Inspector.");
        }
    }

    void HideInteractionUI()
    {
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    void ChangeCrosshairColor(bool isHovering)
    {
        if (!changeCrosshairColor || crosshairObject == null) return;
        crosshairObject.color = isHovering ? crosshairHoverColor : originalCrosshairColor;
    }

    void DisablePlayerControls()
    {
        try
        {
            if (firstPersonController != null)
            {
                firstPersonController.playerCanMove = false;
                firstPersonController.cameraCanMove = false;
            }
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        catch { }
    }

    void EnablePlayerControls()
    {
        try
        {
            if (firstPersonController != null)
            {
                firstPersonController.playerCanMove = true;
                firstPersonController.cameraCanMove = true;
                if (firstPersonController.lockCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.visible = false;
                }
            }
        }
        catch { }
    }

    public bool HasActiveSessionOrFocus()
    {
        if (!isActiveAndEnabled) return false;
        return isActiveSession || playerInRange;
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
        if (pileCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pileCollider.bounds.center, pileCollider.bounds.size);
        }
        else
        {
            // Warning: No collider found
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}



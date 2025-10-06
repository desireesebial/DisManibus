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

        if (interactionUI != null) interactionUI.SetActive(false);

        if (pileSwiper != null)
        {
            pileSwiper.OnPileCleared -= HandlePileCleared;
            pileSwiper.OnPileCleared += HandlePileCleared;
            pileSwiper.OnCancelled -= HandleCancelled;
            pileSwiper.OnCancelled += HandleCancelled;
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
            playerInRange = IsPileUnderCrosshair() && distance <= crosshairDetectionRange;
        }
        else
        {
            playerInRange = distance <= interactionRange;
        }

        if (playerInRange && !wasInRange && !isActiveSession)
        {
            ShowInteractionUI();
            ChangeCrosshairColor(true);
        }
        else if ((!playerInRange || isActiveSession) && wasInRange)
        {
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

        // If cleared or only the last page remains and we keep it, show final note instead of enabling swipe
        if ((pileSwiper.keepLastPage && pileSwiper.IsOnlyLastPageRemaining()) || pileSwiper.IsCleared())
        {
            if (pileSwiper.finalNote != null && pileSwiper.noteUI != null)
            {
                pileSwiper.noteUI.ShowNote(pileSwiper.finalNote);
            }
            return;
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
		// Optionally hide the world pile object immediately after all pages are swiped
		if (hideWorldPileOnCleared)
		{
			GameObject toHide = worldPileObject != null ? worldPileObject : gameObject;
			if (toHide != null) toHide.SetActive(false);
		}

        // Swiping finished; allow note UI to show, and when the player closes the note, re-enable controls.
        // We don't re-enable here immediately to keep controls disabled while the final note is open.
        // If there is no final note or UI, end the session now.
        if (pileSwiper != null && pileSwiper.finalNote != null && pileSwiper.noteUI != null)
        {
            // Hook into the note UI close to restore controls
            pileSwiper.noteUI.OnNoteClosed -= RestoreAfterNote;
            pileSwiper.noteUI.OnNoteClosed += RestoreAfterNote;
        }
        else
        {
            RestoreSession();
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
        if (playerCamera == null) return false;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, pileLayerMask))
        {
            return hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);
        }
        return false;
    }

    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null) interactionTextUI.text = interactionText;
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
}



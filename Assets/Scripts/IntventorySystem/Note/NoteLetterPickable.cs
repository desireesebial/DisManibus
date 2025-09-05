using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoteLetterPickable : MonoBehaviour
{
    // Static variable to track if any note is currently open
    private static bool anyNoteOpen = false;
    
    [Header("Note Data")]
    public NoteLetterSO noteData;
    
    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to read note";
    
    [Header("Crosshair Detection")]
    public bool useCrosshairDetection = true;
    public float crosshairDetectionRange = 10f;
    public LayerMask noteLayerMask = -1;
    
    [Header("Crosshair Feedback")]
    public bool changeCrosshairColor = true;
    public Color crosshairHoverColor = Color.yellow;
    
    [Header("UI References")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionTextUI;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    private bool playerInRange = false;
    private NoteLetterUI noteLetterUI;
    private bool isNoteOpen = false;
    
    // References to player components
    private FirstPersonController firstPersonController;
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    
    // Crosshair feedback
    private Image crosshairObject;
    private Color originalCrosshairColor;
    private bool wasHovering = false;
    
    void Start()
    {
        // Find the NoteLetterUI in the scene
        noteLetterUI = FindObjectOfType<NoteLetterUI>();
        
        if (noteLetterUI == null)
        {
            Debug.LogError("NoteLetterUI not found in scene! Please add it to your Canvas.");
        }
        
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            firstPersonController = player.GetComponent<FirstPersonController>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerCamera = player.GetComponentInChildren<Camera>();
            
            // Get crosshair reference from FirstPersonController
            if (firstPersonController != null)
            {
                crosshairObject = firstPersonController.GetComponentInChildren<Image>();
                if (crosshairObject != null)
                {
                    originalCrosshairColor = crosshairObject.color;
                }
            }
        }
        
        // Hide interaction UI initially
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        // Validate note data
        if (noteData == null)
        {
            Debug.LogError($"NoteLetterPickable on {gameObject.name} has no NoteLetterSO assigned!");
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
            // Use crosshair detection instead of simple distance
            playerInRange = IsNoteUnderCrosshair() && distance <= crosshairDetectionRange;
        }
        else
        {
            // Use simple distance detection
            playerInRange = distance <= interactionRange;
        }
        
        // Only show interaction UI if this note is not already open
        if (playerInRange && !wasInRange && !isNoteOpen)
        {
            ShowInteractionUI();
            ChangeCrosshairColor(true);
        }
        else if ((!playerInRange || isNoteOpen) && wasInRange)
        {
            HideInteractionUI();
            ChangeCrosshairColor(false);
        }
    }
    
    void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (!isNoteOpen)
            {
                OpenNote();
            }
            else
            {
                CloseNote();
            }
        }
    }
    
    void OpenNote()
    {
        // Validate components
        if (noteLetterUI == null)
        {
            Debug.LogError("NoteLetterUI not found! Cannot open note.");
            return;
        }
        
        if (noteData == null)
        {
            Debug.LogError("Note data is null! Cannot open note.");
            return;
        }
        
        // If this note is already open, close it instead
        if (isNoteOpen)
        {
            CloseNote();
            return;
        }
        
        // If any note is open, close it first
        if (anyNoteOpen)
        {
            CloseAnyOpenNote();
        }
        
        // Play open sound
        if (audioSource != null && noteData.openSound != null)
        {
            audioSource.PlayOneShot(noteData.openSound);
        }
        
        // Show the note UI
        noteLetterUI.ShowNote(noteData);
        // Subscribe once to UI close event to restore player state
        noteLetterUI.OnNoteClosed -= HandleNoteUIClose; // prevent double subscription
        noteLetterUI.OnNoteClosed += HandleNoteUIClose;
        isNoteOpen = true;
        anyNoteOpen = true;
        
        // Hide interaction UI
        HideInteractionUI();
        
        // Disable player movement and controls
        DisablePlayerControls();
    }
    
    public void CloseNote()
    {
        if (isNoteOpen)
        {
            // Play close sound
            if (audioSource != null && noteData.closeSound != null)
            {
                audioSource.PlayOneShot(noteData.closeSound);
            }
            
            // Hide the note UI
            if (noteLetterUI != null)
            {
                noteLetterUI.HideNote();
            }
            
            isNoteOpen = false;
            anyNoteOpen = false;
            
            // Re-enable player movement and controls
            EnablePlayerControls();
            
            // Restore crosshair color when closing note
            ChangeCrosshairColor(false);
        }
    }

    private void HandleNoteUIClose()
    {
        // Called when UI has fully closed (after fade). Restore state safely.
        isNoteOpen = false;
        anyNoteOpen = false;
        EnablePlayerControls();
        ChangeCrosshairColor(false);
        // Unsubscribe to avoid leaks
        if (noteLetterUI != null)
        {
            noteLetterUI.OnNoteClosed -= HandleNoteUIClose;
        }
    }
    
    void CloseAnyOpenNote()
    {
        // Find all note pickable objects and close any that are open
        NoteLetterPickable[] allNotes = FindObjectsOfType<NoteLetterPickable>();
        foreach (NoteLetterPickable note in allNotes)
        {
            if (note.isNoteOpen)
            {
                note.isNoteOpen = false;
                note.HideInteractionUI();
                note.ChangeCrosshairColor(false);
                // IMPORTANT: Restore player controls when closing any note
                note.EnablePlayerControls();
            }
        }
        
        // Reset global state
        anyNoteOpen = false;
        
        // Hide the UI immediately
        if (noteLetterUI != null)
        {
            noteLetterUI.HideNoteImmediate();
        }
    }
    
    void DisablePlayerControls()
    {
        try
        {
            Debug.Log("Disabling player controls...");
            
            // Disable FirstPersonController movement
            if (firstPersonController != null)
            {
                firstPersonController.playerCanMove = false;
                firstPersonController.cameraCanMove = false;
                Debug.Log("FirstPersonController controls disabled");
            }
            else
            {
                Debug.LogWarning("FirstPersonController is null!");
            }
            
            // Disable player rigidbody movement
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                Debug.Log("Rigidbody velocity stopped");
            }
            else
            {
                Debug.LogWarning("PlayerRigidbody is null!");
            }
            
            // Unlock cursor and show it for note interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Cursor unlocked and shown");
            
            Debug.Log("Player controls successfully disabled");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error disabling player controls: {e.Message}");
        }
    }
    
    void EnablePlayerControls()
    {
        try
        {
            Debug.Log("Enabling player controls...");
            
            // Re-enable FirstPersonController movement
            if (firstPersonController != null)
            {
                firstPersonController.playerCanMove = true;
                firstPersonController.cameraCanMove = true;
                Debug.Log("FirstPersonController controls enabled");
            }
            else
            {
                Debug.LogWarning("FirstPersonController is null!");
            }
            
            // Re-lock cursor and hide it for FPS gameplay
            if (firstPersonController != null && firstPersonController.lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("Cursor locked and hidden");
            }
            else
            {
                // If lockCursor is false, just hide the cursor but don't lock it
                Cursor.visible = false;
                Debug.Log("Cursor hidden (not locked)");
            }
            
            // Ensure rigidbody is not frozen
            if (playerRigidbody != null)
            {
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                Debug.Log("Rigidbody constraints reset");
            }
            
            Debug.Log("Player controls successfully enabled");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error enabling player controls: {e.Message}");
        }
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
            // Change to hover color
            crosshairObject.color = crosshairHoverColor;
            wasHovering = true;
        }
        else if (!isHovering && wasHovering)
        {
            // Only restore original color if no other note is being hovered
            if (!IsAnyNoteBeingHovered())
            {
                crosshairObject.color = originalCrosshairColor;
            }
            wasHovering = false;
        }
    }
    
    bool IsAnyNoteBeingHovered()
    {
        // Check if any other note is currently being hovered
        NoteLetterPickable[] allNotes = FindObjectsOfType<NoteLetterPickable>();
        foreach (NoteLetterPickable note in allNotes)
        {
            if (note != this && note.playerInRange)
            {
                return true;
            }
        }
        return false;
    }
    
    
    bool IsNoteUnderCrosshair()
    {
        if (playerCamera == null) return false;
        
        // Cast a ray from camera center (crosshair position)
        // This matches exactly where your FirstPersonController crosshair is positioned
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        // Check if ray hits this note object
        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, noteLayerMask))
        {
            // Check if the hit object is this note or its child
            return hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);
        }
        
        return false;
    }
    
    
    // Public method to force reset player controls (useful for debugging)
    [ContextMenu("Force Reset Player Controls")]
    public void ForceResetPlayerControls()
    {
        Debug.Log("Force resetting player controls...");
        EnablePlayerControls();
    }
    
    void OnDrawGizmosSelected()
    {
        // Gizmos for interaction range visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Gizmos for crosshair detection range
        if (useCrosshairDetection)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, crosshairDetectionRange);
        }
    }
}

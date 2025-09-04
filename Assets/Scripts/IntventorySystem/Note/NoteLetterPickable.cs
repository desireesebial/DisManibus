using UnityEngine;
using TMPro;

public class NoteLetterPickable : MonoBehaviour
{
    [Header("Note Data")]
    public NoteLetterSO noteData;
    
    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Press E to read note";
    
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
        playerInRange = distance <= interactionRange;
        
        // Show/hide interaction UI
        if (playerInRange && !wasInRange)
        {
            ShowInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
        }
    }
    
    void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !isNoteOpen)
        {
            OpenNote();
        }
    }
    
    void OpenNote()
    {
        if (noteLetterUI != null && noteData != null)
        {
            // Play open sound
            if (audioSource != null && noteData.openSound != null)
            {
                audioSource.PlayOneShot(noteData.openSound);
            }
            
            // Show the note UI
            noteLetterUI.ShowNote(noteData);
            isNoteOpen = true;
            
            // Hide interaction UI
            HideInteractionUI();
            
            // Disable player movement and controls
            DisablePlayerControls();
        }
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
            
            // Re-enable player movement and controls
            EnablePlayerControls();
        }
    }
    
    void DisablePlayerControls()
    {
        // Disable FirstPersonController movement
        if (firstPersonController != null)
        {
            firstPersonController.playerCanMove = false;
            firstPersonController.cameraCanMove = false;
        }
        
        // Disable player rigidbody movement
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Unlock cursor and show it for note interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void EnablePlayerControls()
    {
        // Re-enable FirstPersonController movement
        if (firstPersonController != null)
        {
            firstPersonController.playerCanMove = true;
            firstPersonController.cameraCanMove = true;
        }
        
        // Re-lock cursor and hide it for FPS gameplay
        if (firstPersonController != null && firstPersonController.lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
    
    void OnDrawGizmosSelected()
    {
        // Gizmos for interaction range visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

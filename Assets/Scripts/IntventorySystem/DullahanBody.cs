using UnityEngine;
using System.Collections;

public class DullahanBody : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public int requiredHeadID = 1; // ID of the real head
    public bool puzzleCompleted = false;
    
    [Header("Door Reference")]
    public Door finalDoor; // Reference to the door that opens when puzzle is complete
    
    [Header("Interaction")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.F;
    public string interactionText = "Press F to attach head";
    
    [Header("Visual Effects")]
    public GameObject headAttachmentPoint; // Where the head will be attached
    public GameObject attachedHeadVisual; // Visual representation of attached head
    public Light bodyLight;
    public Color completedColor = Color.green;
    public Color normalColor = Color.white;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip headAttachSound;
    public AudioClip puzzleCompleteSound;
    
    [Header("UI")]
    public GameObject interactionUI;
    public TMPro.TextMeshProUGUI interactionTextUI;
    
    private bool playerInRange = false;
    private Transform player;
    private DullahanHeadInventory headInventory;
    private DullahanAudioManager audioManager;
    private bool isInitialized = false;
    
    void Start()
    {
        // Find references
        FindReferences();
        
        // Setup initial state
        SetupInitialState();
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        CheckPlayerDistance();
        HandleInteraction();
    }
    
    private void FindReferences()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Find head inventory
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
        
        // Find audio manager
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
        
        // Find final door if not assigned
        if (finalDoor == null)
        {
            Door[] doors = FindObjectsOfType<Door>();
            foreach (Door door in doors)
            {
                if (door.RequiredKeyID == 999) // Assuming 999 is the special key for final door
                {
                    finalDoor = door;
                    break;
                }
            }
        }
        
        // Setup audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void SetupInitialState()
    {
        // Hide attached head visual initially
        if (attachedHeadVisual != null)
            attachedHeadVisual.SetActive(false);
        
        // Setup body light
        if (bodyLight != null)
            bodyLight.color = normalColor;
        
        // Hide interaction UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    void CheckPlayerDistance()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        // Show/hide interaction UI
        if (playerInRange && !wasInRange && !puzzleCompleted)
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
        if (!playerInRange || puzzleCompleted) return;
        
        if (Input.GetKeyDown(interactionKey))
        {
            TryAttachHead();
        }
    }
    
    public bool AttachHead(DullahanHeadSO headData)
    {
        if (puzzleCompleted || headData == null) return false;
        
        // Check if this is the correct head
        if (headData.headID != requiredHeadID)
        {
            Debug.Log("Wrong head! This is not the real head.");
            PlayWrongHeadSound();
            
            // Notify event manager about wrong head
            NotifyEventManager(headData.headType);
            return false;
        }
        
        // Complete the puzzle
        CompletePuzzle(headData);
        
        // Notify event manager about correct head
        NotifyEventManager(headData.headType);
        return true;
    }
    
    private void NotifyEventManager(HeadType headType)
    {
        // Find and notify the event manager
        DullahanChaseEventManager eventManager = FindObjectOfType<DullahanChaseEventManager>();
        if (eventManager != null)
        {
            eventManager.OnHeadAttached(headType);
        }
    }
    
    private void TryAttachHead()
    {
        if (headInventory == null || !headInventory.HasHeads()) 
        {
            Debug.Log("No heads in inventory!");
            return;
        }
        
        DullahanHeadSO currentHead = headInventory.GetCurrentHead();
        if (currentHead == null) return;
        
        // Try to attach the head
        if (AttachHead(currentHead))
        {
            // Remove head from inventory
            headInventory.headInventoryList.Remove(currentHead);
            
            // Update head inventory
            if (headInventory.headInventoryList.Count > 0)
            {
                headInventory.selectedHeadIndex = 0;
                headInventory.NewHeadSelected();
            }
            else
            {
                headInventory.selectedHeadIndex = -1;
                headInventory.DeactivateAllHeads();
            }
        }
    }
    
    private void CompletePuzzle(DullahanHeadSO headData)
    {
        puzzleCompleted = true;
        
        // Show attached head visual
        if (attachedHeadVisual != null)
            attachedHeadVisual.SetActive(true);
        
        // Change body light color
        if (bodyLight != null)
            bodyLight.color = completedColor;
        
        // Play completion sound
        PlayPuzzleCompleteSound();
        
        // Open the final door
        if (finalDoor != null)
        {
            finalDoor.UnlockDoor();
            Debug.Log("Final door unlocked!");
        }
        else
        {
            Debug.LogWarning("No final door assigned to DullahanBody!");
        }
        
        // Hide interaction UI
        HideInteractionUI();
        
        Debug.Log($"Puzzle completed! {headData.headName} attached to Dullahan body.");
    }
    
    private void PlayWrongHeadSound()
    {
        // Try audio manager first
        if (audioManager != null)
        {
            audioManager.PlayWrongHeadSound();
        }
        // Fallback to local audio
        else if (audioSource != null && headAttachSound != null)
        {
            audioSource.PlayOneShot(headAttachSound);
        }
    }
    
    private void PlayPuzzleCompleteSound()
    {
        // Try audio manager first
        if (audioManager != null)
        {
            audioManager.PlayPuzzleCompleteSound();
        }
        // Fallback to local audio
        else if (audioSource != null && puzzleCompleteSound != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSound);
        }
    }
    
    void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionTextUI != null)
                interactionTextUI.text = interactionText;
        }
    }
    
    void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !puzzleCompleted)
        {
            playerInRange = true;
            ShowInteractionUI();
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionUI();
        }
    }
    
    // Public methods for other scripts
    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
    
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        
        if (attachedHeadVisual != null)
            attachedHeadVisual.SetActive(false);
        
        if (bodyLight != null)
            bodyLight.color = normalColor;
        
        if (finalDoor != null)
            finalDoor.LockDoor();
    }
}

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
        if (headData.headID == requiredHeadID)
        {
            Debug.Log("Correct head! Puzzle completed.");
            
            // Complete the puzzle
            CompletePuzzle(headData);
            
            // Notify event manager about correct head
            NotifyEventManager(headData.headType);
            return true;
        }
        else
        {
            Debug.Log($"Fake head attached: {headData.headType}. Applying effects and consuming head.");
            
            // Handle fake head - consume it and apply effects
            HandleFakeHeadAttachment(headData);
            
            // Notify event manager about fake head
            NotifyEventManager(headData.headType);
            return true; // Return true so head is consumed from inventory
        }
    }
    
    private void NotifyEventManager(HeadType headType)
    {
        // Find and notify the event manager
        DullahanChaseEventManager eventManager = FindObjectOfType<DullahanChaseEventManager>();
        if (eventManager != null)
        {
            if (headType == HeadType.Real)
            {
                eventManager.OnRealHeadAttachedToBody();
            }
            else
            {
                eventManager.OnHeadAttached(headType);
            }
        }
    }
    
    private void HandleFakeHeadAttachment(DullahanHeadSO headData)
    {
        if (headData == null) return;
        
        Debug.Log($"Handling fake head attachment: {headData.headName}");
        
        // Apply effects to player
        ApplyFakeHeadEffectsToPlayer(headData);
        
        // Apply effects to Dullahan
        ApplyFakeHeadEffectsToDullahan(headData);
        
        // Play fake head attachment sound
        PlayWrongHeadSound();
        
        // Show temporary visual feedback
        StartCoroutine(ShowFakeHeadFeedback(headData));
    }
    
    private void ApplyFakeHeadEffectsToPlayer(DullahanHeadSO headData)
    {
        if (headData == null || !headData.hasEffect) return;
        
        // Find the effect manager
        DullahanHeadEffectManager effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        if (effectManager != null)
        {
            effectManager.ApplyHeadEffect(headData);
            Debug.Log($"Applied {headData.effectType} effect to player from {headData.headName}");
        }
        else
        {
            Debug.LogWarning("No DullahanHeadEffectManager found for player effects!");
        }
    }
    
    private void ApplyFakeHeadEffectsToDullahan(DullahanHeadSO headData)
    {
        if (headData == null || !headData.hasEffect) return;
        
        // Find the Dullahan chase system
        DullahanChaseSystem dullahanChase = FindObjectOfType<DullahanChaseSystem>();
        if (dullahanChase != null)
        {
            ApplyDullahanEffects(dullahanChase, headData);
            Debug.Log($"Applied {headData.effectType} effect to Dullahan from {headData.headName}");
        }
        else
        {
            Debug.LogWarning("No DullahanChaseSystem found for Dullahan effects!");
        }
    }
    
    private void ApplyDullahanEffects(DullahanChaseSystem dullahanChase, DullahanHeadSO headData)
    {
        switch (headData.effectType)
        {
            case EffectType.FearEffect:
                // Increase Dullahan chase intensity
                dullahanChase.SetChaseIntensity(dullahanChase.GetCurrentIntensity() + headData.effectStrength);
                break;
                
            case EffectType.CalmEffect:
                // Decrease Dullahan chase intensity
                dullahanChase.SetChaseIntensity(Mathf.Max(0, dullahanChase.GetCurrentIntensity() - headData.effectStrength));
                break;
                
            case EffectType.SpeedBoost:
                // Increase Dullahan movement speed
                float currentMinSpeed = dullahanChase.minChaseSpeed;
                float currentMaxSpeed = dullahanChase.maxChaseSpeed;
                float speedBoost = headData.effectStrength;
                dullahanChase.SetChaseSpeed(currentMinSpeed + speedBoost, currentMaxSpeed + speedBoost);
                break;
                
            case EffectType.SpeedDebuff:
                // Decrease Dullahan movement speed
                float currentMinSpeedDebuff = dullahanChase.minChaseSpeed;
                float currentMaxSpeedDebuff = dullahanChase.maxChaseSpeed;
                float speedDebuff = headData.effectStrength;
                dullahanChase.SetChaseSpeed(Mathf.Max(1f, currentMinSpeedDebuff - speedDebuff), Mathf.Max(2f, currentMaxSpeedDebuff - speedDebuff));
                break;
                
            default:
                Debug.Log($"Effect type {headData.effectType} not implemented for Dullahan");
                break;
        }
    }
    
    private System.Collections.IEnumerator ShowFakeHeadFeedback(DullahanHeadSO headData)
    {
        // Flash the body light to indicate fake head was consumed
        if (bodyLight != null)
        {
            Color originalColor = bodyLight.color;
            bodyLight.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            bodyLight.color = originalColor;
        }
        
        // Optional: Show temporary particle effect or other visual feedback
        yield return new WaitForSeconds(1f);
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
            // Remove head from inventory using compatibility method
            headInventory.RemoveFromInventoryList(currentHead);
            
            // Update head inventory
            if (headInventory.inventoryList.Count > 0)
            {
                headInventory.selectedItem = 0;
                headInventory.NewItemSelected();
            }
            else
            {
                headInventory.selectedItem = -1;
                headInventory.DeactivateAllItems();
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

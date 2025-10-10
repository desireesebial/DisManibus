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
    
    [Header("Fake Head Visuals")] 
    public bool showFakeHeadPlacement = true; // briefly show fake head when placed
    public float fakeHeadVisualDuration = 1f; // seconds before it disappears (was 0.75f)
    private GameObject temporaryFakeHeadInstance;
    
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
            Debug.Log($"DullahanBody: Player entered interaction range (distance: {distance:F2})");
            ShowInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            Debug.Log($"DullahanBody: Player left interaction range (distance: {distance:F2})");
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
        Debug.Log("[AttachHead] ═══ METHOD CALLED ═══");
        
        if (puzzleCompleted)
        {
            Debug.LogWarning("[AttachHead] Puzzle already completed, cannot attach more heads");
            return false;
        }
        
        if (headData == null)
        {
            Debug.LogError("[AttachHead] ✗ CRITICAL ERROR: headData is null!");
            return false;
        }
        
        Debug.Log($"[AttachHead] Processing head: {headData.headName}");
        Debug.Log($"[AttachHead]   - Head ID: {headData.headID}");
        Debug.Log($"[AttachHead]   - Required ID: {requiredHeadID}");
        Debug.Log($"[AttachHead]   - Head Type: {headData.headType}");
        
        // Check if this is the correct head
        if (headData.headID == requiredHeadID)
        {
            Debug.Log($"[AttachHead] ╔══════════════════════════════════════╗");
            Debug.Log($"[AttachHead] ║ ✓✓✓ CORRECT HEAD ATTACHED! ✓✓✓    ║");
            Debug.Log($"[AttachHead] ║ Head ID {headData.headID} matches required {requiredHeadID}   ║");
            Debug.Log($"[AttachHead] ║ PUZZLE COMPLETED!                   ║");
            Debug.Log($"[AttachHead] ╚══════════════════════════════════════╝");
            
            // Complete the puzzle
            CompletePuzzle(headData);
            
            // Notify event manager about correct head
            NotifyEventManager(headData.headType);
            
            Debug.Log("[AttachHead] Returning TRUE - head will be consumed from inventory");
            return true;
        }
        else
        {
            Debug.Log($"[AttachHead] ╔══════════════════════════════════════╗");
            Debug.Log($"[AttachHead] ║ ✗✗✗ WRONG HEAD ATTACHED! ✗✗✗       ║");
            Debug.Log($"[AttachHead] ║ Type: {headData.headType,-20} ║");
            Debug.Log($"[AttachHead] ║ ID Mismatch: {headData.headID} != {requiredHeadID,-12} ║");
            Debug.Log($"[AttachHead] ║ APPLYING NEGATIVE EFFECTS!          ║");
            Debug.Log($"[AttachHead] ╚══════════════════════════════════════╝");
            
            // Handle fake head - consume it and apply effects
            HandleFakeHeadAttachment(headData);
            
            // Notify event manager about fake head
            NotifyEventManager(headData.headType);
            
            Debug.Log("[AttachHead] Returning TRUE - wrong head will be consumed from inventory");
            return true; // Return true so head is consumed from inventory
        }
    }
    
    private void NotifyEventManager(HeadType headType)
    {
        Debug.Log($"DullahanBody: Notifying event managers about {headType} head attachment");
        
        // Try to find DullahanChaseEventManager first
        DullahanChaseEventManager chaseEventManager = FindObjectOfType<DullahanChaseEventManager>();
        if (chaseEventManager != null)
        {
            Debug.Log("DullahanBody: Found DullahanChaseEventManager, notifying...");
            if (headType == HeadType.Real)
            {
                chaseEventManager.OnRealHeadAttachedToBody();
            }
            else
            {
                chaseEventManager.OnHeadAttached(headType);
            }
        }
        else
        {
            Debug.Log("DullahanBody: No DullahanChaseEventManager found in scene");
        }
        
        // Also try Floor2EndingEventManager (for simpler setup)
        Floor2EndingEventManager floor2EventManager = FindObjectOfType<Floor2EndingEventManager>();
        if (floor2EventManager != null)
        {
            Debug.Log("DullahanBody: Found Floor2EndingEventManager, notifying...");
            if (headType == HeadType.Real)
            {
                floor2EventManager.OnRealHeadAttachedToBody();
            }
        }
        else
        {
            Debug.Log("DullahanBody: No Floor2EndingEventManager found in scene");
        }
        
        // Warn if no event managers found
        if (chaseEventManager == null && floor2EventManager == null)
        {
            Debug.LogWarning("DullahanBody: No event managers found! Head attachment will work but no ending events will trigger.");
        }
    }
    
    private void HandleFakeHeadAttachment(DullahanHeadSO headData)
    {
        if (headData == null) return;
        
        Debug.Log($"╔═══════════════════════════════════════════════════╗");
        Debug.Log($"║ WRONG HEAD ATTACHED: {headData.headName}");
        Debug.Log($"║ Head Type: {headData.headType}");
        Debug.Log($"║ Has Effect: {headData.hasEffect}");
        if (headData.hasEffect)
        {
            Debug.Log($"║ Effect Type: {headData.effectType}");
            Debug.Log($"║ Effect Strength: {headData.effectStrength}");
            Debug.Log($"║ Effect Duration: {headData.effectDuration}s");
        }
        Debug.Log($"╚═══════════════════════════════════════════════════╝");
        
        // Spawn a temporary visual of the fake head at the attachment point
        TryShowTemporaryFakeHead(headData);
        
        // Apply effects to player (DEBUFF)
        ApplyFakeHeadEffectsToPlayer(headData);
        
        // Apply effects to Dullahan (BUFF - makes chase harder)
        ApplyFakeHeadEffectsToDullahan(headData);
        
        // Play fake head attachment sound
        PlayWrongHeadSound();
        
        // Show temporary visual feedback
        StartCoroutine(ShowFakeHeadFeedback(headData));
    }

    private void TryShowTemporaryFakeHead(DullahanHeadSO headData)
    {
        if (!showFakeHeadPlacement)
        {
            Debug.LogWarning("[Visual] showFakeHeadPlacement is disabled - no visual will be shown");
            return;
        }
        
        if (headAttachmentPoint == null)
        {
            Debug.LogError("[Visual] headAttachmentPoint is NULL! Cannot show head visual. Please assign it in the inspector.");
            return;
        }
        
        if (headData.headPrefab == null)
        {
            Debug.LogWarning($"[Visual] {headData.headName} has no headPrefab assigned! Cannot show visual.");
            return;
        }

        // Clean up any previous temp instance
        if (temporaryFakeHeadInstance != null)
        {
            Debug.Log("[Visual] Destroying previous temporary head instance");
            Destroy(temporaryFakeHeadInstance);
            temporaryFakeHeadInstance = null;
        }

        Debug.Log($"[Visual] ★ SPAWNING TEMPORARY HEAD: {headData.headName} at {headAttachmentPoint.name}");
        Debug.Log($"[Visual] Head will be visible for {fakeHeadVisualDuration} seconds before disappearing");
        
        // Instantiate under the attachment point and align
        temporaryFakeHeadInstance = Instantiate(headData.headPrefab, headAttachmentPoint.transform);
        temporaryFakeHeadInstance.transform.localPosition = Vector3.zero;
        temporaryFakeHeadInstance.transform.localRotation = Quaternion.identity;
        temporaryFakeHeadInstance.transform.localScale = Vector3.one;
        temporaryFakeHeadInstance.name = $"TEMP_WRONG_HEAD_{headData.headName}";
        
        Debug.Log($"[Visual] ✓ Temporary head spawned successfully: {temporaryFakeHeadInstance.name}");

        // Auto-remove after a short duration
        StartCoroutine(RemoveTemporaryFakeHeadAfterDelay(fakeHeadVisualDuration));
    }

    private IEnumerator RemoveTemporaryFakeHeadAfterDelay(float delay)
    {
        Debug.Log($"[Visual] Waiting {delay} seconds before removing temporary head...");
        yield return new WaitForSeconds(delay);
        
        if (temporaryFakeHeadInstance != null)
        {
            Debug.Log($"[Visual] ✗ REMOVING temporary head: {temporaryFakeHeadInstance.name}");
            Destroy(temporaryFakeHeadInstance);
            temporaryFakeHeadInstance = null;
        }
        else
        {
            Debug.LogWarning("[Visual] Temporary head instance was already null when trying to remove it");
        }
    }
    
    private void ApplyFakeHeadEffectsToPlayer(DullahanHeadSO headData)
    {
        if (headData == null)
        {
            Debug.LogError("[Effect] headData is null - cannot apply player effects");
            return;
        }
        
        if (!headData.hasEffect)
        {
            Debug.Log($"[Effect] {headData.headName} has no effect to apply to player");
            return;
        }
        
        Debug.Log($"[Effect] ► Applying PLAYER DEBUFF from {headData.headName}:");
        Debug.Log($"[Effect]   - Effect Type: {headData.effectType}");
        Debug.Log($"[Effect]   - Strength: {headData.effectStrength}");
        Debug.Log($"[Effect]   - Duration: {headData.effectDuration}s");
        
        // Find the effect manager
        DullahanHeadEffectManager effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        if (effectManager != null)
        {
            effectManager.ApplyHeadEffect(headData);
            Debug.Log($"[Effect] ✓ Successfully applied {headData.effectType} effect to player");
        }
        else
        {
            Debug.LogError("[Effect] ✗ No DullahanHeadEffectManager found! Player effects will NOT work!");
        }
    }
    
    private void ApplyFakeHeadEffectsToDullahan(DullahanHeadSO headData)
    {
        if (headData == null)
        {
            Debug.LogError("[Effect] headData is null - cannot apply Dullahan effects");
            return;
        }
        
        if (!headData.hasEffect)
        {
            Debug.Log($"[Effect] {headData.headName} has no effect to apply to Dullahan");
            return;
        }
        
        Debug.Log($"[Effect] ► Applying DULLAHAN BUFF from {headData.headName}:");
        Debug.Log($"[Effect]   - Effect Type: {headData.effectType}");
        Debug.Log($"[Effect]   - Strength: {headData.effectStrength}");
        Debug.Log($"[Effect]   - This will make Dullahan MORE DANGEROUS");
        
        // Find the Dullahan chase system
        DullahanChaseSystem dullahanChase = FindObjectOfType<DullahanChaseSystem>();
        if (dullahanChase != null)
        {
            ApplyDullahanEffects(dullahanChase, headData);
            Debug.Log($"[Effect] ✓ Successfully applied {headData.effectType} effect to Dullahan");
        }
        else
        {
            Debug.LogWarning("[Effect] ⚠ No DullahanChaseSystem found! Dullahan buffs will NOT work!");
        }
    }
    
    private void ApplyDullahanEffects(DullahanChaseSystem dullahanChase, DullahanHeadSO headData)
    {
        Debug.Log($"[Dullahan Effect] Processing effect: {headData.effectType}");
        
        switch (headData.effectType)
        {
            case EffectType.FearEffect:
                // Increase Dullahan chase intensity (MAKES DULLAHAN MORE AGGRESSIVE)
                float oldIntensity = dullahanChase.GetCurrentIntensity();
                float newIntensity = oldIntensity + headData.effectStrength;
                dullahanChase.SetChaseIntensity(newIntensity);
                Debug.Log($"[Dullahan Effect] FearEffect: Intensity {oldIntensity:F2} → {newIntensity:F2} (+{headData.effectStrength:F2})");
                break;
                
            case EffectType.CalmEffect:
                // Decrease Dullahan chase intensity (MAKES DULLAHAN LESS AGGRESSIVE)
                float oldCalmIntensity = dullahanChase.GetCurrentIntensity();
                float newCalmIntensity = Mathf.Max(0, oldCalmIntensity - headData.effectStrength);
                dullahanChase.SetChaseIntensity(newCalmIntensity);
                Debug.Log($"[Dullahan Effect] CalmEffect: Intensity {oldCalmIntensity:F2} → {newCalmIntensity:F2} (-{headData.effectStrength:F2})");
                break;
                
            case EffectType.SpeedBoost:
                // Increase Dullahan movement speed (MAKES DULLAHAN FASTER - BAD FOR PLAYER)
                float currentMinSpeed = dullahanChase.minChaseSpeed;
                float currentMaxSpeed = dullahanChase.maxChaseSpeed;
                float speedBoost = headData.effectStrength;
                float newMinSpeed = currentMinSpeed + speedBoost;
                float newMaxSpeed = currentMaxSpeed + speedBoost;
                dullahanChase.SetChaseSpeed(newMinSpeed, newMaxSpeed);
                Debug.Log($"[Dullahan Effect] SpeedBoost: Min {currentMinSpeed:F1} → {newMinSpeed:F1}, Max {currentMaxSpeed:F1} → {newMaxSpeed:F1}");
                break;
                
            case EffectType.SpeedDebuff:
                // Decrease Dullahan movement speed (MAKES DULLAHAN SLOWER - GOOD FOR PLAYER)
                float currentMinSpeedDebuff = dullahanChase.minChaseSpeed;
                float currentMaxSpeedDebuff = dullahanChase.maxChaseSpeed;
                float speedDebuff = headData.effectStrength;
                float newMinSpeedDebuff = Mathf.Max(1f, currentMinSpeedDebuff - speedDebuff);
                float newMaxSpeedDebuff = Mathf.Max(2f, currentMaxSpeedDebuff - speedDebuff);
                dullahanChase.SetChaseSpeed(newMinSpeedDebuff, newMaxSpeedDebuff);
                Debug.Log($"[Dullahan Effect] SpeedDebuff: Min {currentMinSpeedDebuff:F1} → {newMinSpeedDebuff:F1}, Max {currentMaxSpeedDebuff:F1} → {newMaxSpeedDebuff:F1}");
                break;
                
            default:
                Debug.LogWarning($"[Dullahan Effect] Effect type {headData.effectType} not implemented for Dullahan");
                break;
        }
    }
    
    private System.Collections.IEnumerator ShowFakeHeadFeedback(DullahanHeadSO headData)
    {
        Debug.Log($"[Visual Feedback] Showing wrong head feedback for {headData.headName}");
        
        // Flash the body light MULTIPLE TIMES to indicate wrong head was attached
        if (bodyLight != null)
        {
            Color originalColor = bodyLight.color;
            
            // Flash red 3 times to make it very obvious
            for (int i = 0; i < 3; i++)
            {
                bodyLight.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                bodyLight.color = originalColor;
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        Debug.Log($"[Visual Feedback] Wrong head feedback complete - head will disappear after {fakeHeadVisualDuration}s total");
    }
    
    private void TryAttachHead()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("► PLAYER PRESSED F TO ATTACH HEAD TO DULLAHAN BODY");
        Debug.Log("═══════════════════════════════════════════════════════════");
        
        if (headInventory == null)
        {
            Debug.LogError("✗ CRITICAL: headInventory is null! Cannot attach head. Make sure DullahanHeadInventory component exists.");
            return;
        }
        
        if (!headInventory.HasHeads()) 
        {
            Debug.Log("✗ No heads in inventory - player has nothing to attach");
            return;
        }
        
        DullahanHeadSO currentHead = headInventory.GetCurrentHead();
        if (currentHead == null)
        {
            Debug.LogWarning("✗ Current head is null (no head selected) - player may need to select a head first");
            return;
        }
        
        Debug.Log($"[Attach] Current head in hand: {currentHead.headName}");
        Debug.Log($"[Attach]   - Type: {currentHead.headType}");
        Debug.Log($"[Attach]   - ID: {currentHead.headID}");
        Debug.Log($"[Attach]   - Required ID: {requiredHeadID}");
        
        // Store the current inventory state for debugging
        int inventoryCountBefore = headInventory.inventoryList.Count;
        Debug.Log($"[Inventory] Before attachment: {inventoryCountBefore} heads in inventory");
        
        // Try to attach the head
        bool attachmentSuccess = AttachHead(currentHead);
        Debug.Log($"[Attach] AttachHead() returned: {attachmentSuccess}");
        
        if (attachmentSuccess)
        {
            Debug.Log($"[Inventory] ★ REMOVING HEAD FROM INVENTORY: {currentHead.headName}");
            
            // Remove head from inventory using compatibility method
            headInventory.RemoveFromInventoryList(currentHead);
            
            int inventoryCountAfter = headInventory.inventoryList.Count;
            Debug.Log($"[Inventory] After removal: {inventoryCountAfter} heads in inventory (was {inventoryCountBefore})");
            
            // Update head inventory selection
            if (headInventory.inventoryList.Count > 0)
            {
                headInventory.selectedItem = 0;
                headInventory.NewItemSelected();
                Debug.Log($"[Inventory] Switched to next head: {headInventory.GetCurrentHead()?.headName ?? "None"}");
            }
            else
            {
                headInventory.selectedItem = -1;
                headInventory.DeactivateAllItems();
                Debug.Log($"[Inventory] No more heads in inventory - all items deactivated");
            }
            
            Debug.Log("═══════════════════════════════════════════════════════════");
            Debug.Log("✓ HEAD ATTACHMENT COMPLETE");
            Debug.Log("═══════════════════════════════════════════════════════════");
        }
        else
        {
            Debug.LogError($"✗ AttachHead() returned false for {currentHead.headName} - head was NOT consumed!");
            Debug.Log("═══════════════════════════════════════════════════════════");
        }
    }
    
    private void CompletePuzzle(DullahanHeadSO headData)
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"★★★ COMPLETING PUZZLE WITH CORRECT HEAD: {headData.headName} ★★★");
        Debug.Log("═══════════════════════════════════════════════════════════");
        
        puzzleCompleted = true;
        
        // Show attached head visual (permanent, unlike fake heads)
        if (attachedHeadVisual != null)
        {
            attachedHeadVisual.SetActive(true);
            Debug.Log("[Puzzle Complete] ✓ Showing permanent attached head visual");
        }
        else
        {
            Debug.LogWarning("[Puzzle Complete] attachedHeadVisual is null - no visual will be shown!");
        }
        
        // Change body light color to indicate success
        if (bodyLight != null)
        {
            bodyLight.color = completedColor;
            Debug.Log($"[Puzzle Complete] ✓ Changed body light to completion color: {completedColor}");
        }
        
        // Play completion sound
        PlayPuzzleCompleteSound();
        
        // Open the final door
        if (finalDoor != null)
        {
            finalDoor.UnlockDoor();
            Debug.Log("[Puzzle Complete] ✓ Final door unlocked!");
        }
        else
        {
            Debug.LogWarning("[Puzzle Complete] No final door assigned - door won't open automatically");
        }
        
        // Hide interaction UI
        HideInteractionUI();
        Debug.Log("[Puzzle Complete] ✓ Interaction UI hidden");
        
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"★★★ PUZZLE COMPLETE! {headData.headName} permanently attached! ★★★");
        Debug.Log("═══════════════════════════════════════════════════════════");
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

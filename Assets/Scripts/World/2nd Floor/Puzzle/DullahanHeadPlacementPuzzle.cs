using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles the Dullahan head placement puzzle on the 2nd floor.
/// The player must place the correct head onto the Dullahan's body.
/// The placeholder is initially invisible and becomes visible when a head is placed.
/// </summary>
public class DullahanHeadPlacementPuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The ID of the correct head required to complete the puzzle")]
    public int requiredHeadID = 1; // ID of the real head
    
    [Tooltip("Is the puzzle already completed?")]
    public bool puzzleCompleted = false;
    
    [Header("Interaction Settings")]
    [Tooltip("Maximum distance from which player can interact")]
    public float interactionRange = 3f;
    
    [Tooltip("Key to press to place head")]
    public KeyCode interactionKey = KeyCode.F;
    
    [Tooltip("Text shown when player can interact")]
    public string interactionPrompt = "Press F to place head on Dullahan's body";
    
    [Tooltip("Text shown when player has no head")]
    public string noHeadPrompt = "You need a head to place here";
    
    [Tooltip("Use raycast-based interaction instead of distance check")]
    public bool useRaycastInteraction = true;
    
    [Header("Placeholder Settings")]
    [Tooltip("The placeholder object that shows where the head will be placed")]
    public GameObject headPlaceholder;
    
    [Tooltip("Is the placeholder initially visible?")]
    public bool placeholderInitiallyVisible = false;
    
    [Tooltip("Material for placeholder when no head is placed (semi-transparent)")]
    public Material placeholderEmptyMaterial;
    
    [Tooltip("Material for placeholder when hovering with correct head")]
    public Material placeholderValidMaterial;
    
    [Tooltip("Material for placeholder when hovering with wrong head")]
    public Material placeholderInvalidMaterial;
    
    [Header("Head Attachment")]
    [Tooltip("Transform where the head will be attached (usually the neck)")]
    public Transform headAttachmentPoint;
    
    [Tooltip("The visual object that represents the attached head")]
    public GameObject attachedHeadVisual;
    
    [Tooltip("Show fake heads briefly when wrong head is placed")]
    public bool showFakeHeadBriefly = true;
    
    [Tooltip("Duration to show wrong head before removing it")]
    public float fakeHeadDisplayDuration = 2f;
    
    [Header("Visual Effects")]
    [Tooltip("Light that activates when puzzle is completed")]
    public Light completionLight;
    
    [Tooltip("Color of light when puzzle is completed")]
    public Color completedLightColor = Color.green;
    
    [Tooltip("Particle effect when head is placed")]
    public ParticleSystem placementParticles;
    
    [Tooltip("Particle effect when puzzle is completed")]
    public ParticleSystem completionParticles;
    
    [Header("Audio")]
    [Tooltip("Audio source for playing sounds")]
    public AudioSource audioSource;
    
    [Tooltip("Sound when correct head is placed")]
    public AudioClip correctHeadSound;
    
    [Tooltip("Sound when wrong head is placed")]
    public AudioClip wrongHeadSound;
    
    [Tooltip("Sound when puzzle is completed")]
    public AudioClip puzzleCompleteSound;
    
    [Header("UI")]
    [Tooltip("UI canvas or panel for interaction prompt")]
    public GameObject interactionUI;
    
    [Tooltip("Text component for interaction prompt")]
    public TextMeshProUGUI interactionText;
    
    [Header("Rewards")]
    [Tooltip("Door to unlock when puzzle is completed")]
    public Door rewardDoor;
    
    [Tooltip("Items to spawn when puzzle is completed")]
    public GameObject[] rewardItems;
    
    [Tooltip("Transform where reward items spawn")]
    public Transform rewardSpawnPoint;
    
    [Header("Dullahan Chase Integration")]
    [Tooltip("Stop Dullahan from moving when player has a head")]
    public bool freezeDullahanWhenPlayerHasHead = true;
    
    [Tooltip("Reference to Dullahan Chase System (auto-found if not assigned)")]
    public DullahanChaseSystem dullahanChaseSystem;
    
    [Tooltip("Reference to NavMeshAgent to freeze (auto-found if not assigned)")]
    public UnityEngine.AI.NavMeshAgent dullahanAgent;
    
    // Private variables
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera playerCamera;
    private DullahanHeadInventory headInventory;
    private Renderer placeholderRenderer;
    private Material originalPlaceholderMaterial;
    private GameObject currentFakeHeadInstance;
    private bool isShowingPrompt = false;
    private bool isDullahanFrozen = false;
    private bool playerPreviouslyHadHead = false;
    
    void Start()
    {
        InitializeComponents();
        SetupPlaceholder();
        SetupVisuals();
    }
    
    void Update()
    {
        if (puzzleCompleted) return;
        
        // Check if player has a head and freeze/unfreeze Dullahan accordingly
        if (freezeDullahanWhenPlayerHasHead)
        {
            CheckAndFreezeDullahan();
        }
        
        if (useRaycastInteraction)
        {
            CheckRaycastInteraction();
        }
        else
        {
            CheckDistanceInteraction();
        }
        
        HandleInteractionInput();
    }
    
    private void InitializeComponents()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerCamera = playerObj.GetComponentInChildren<Camera>();
            
            // Fallback to Camera.main if no camera found in player hierarchy
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
        
        // Find head inventory
        headInventory = FindObjectOfType<DullahanHeadInventory>();
        if (headInventory == null)
        {
            Debug.LogError("[DullahanHeadPlacementPuzzle] DullahanHeadInventory not found in scene!");
        }
        
        // Find Dullahan Chase System if not assigned
        if (dullahanChaseSystem == null)
        {
            dullahanChaseSystem = FindObjectOfType<DullahanChaseSystem>();
        }
        
        // Find Dullahan NavMeshAgent if not assigned
        if (dullahanAgent == null)
        {
            GameObject dullahanObj = GameObject.FindGameObjectWithTag("Dullahan");
            if (dullahanObj != null)
            {
                dullahanAgent = dullahanObj.GetComponent<UnityEngine.AI.NavMeshAgent>();
            }
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get placeholder renderer
        if (headPlaceholder != null)
        {
            placeholderRenderer = headPlaceholder.GetComponent<Renderer>();
            if (placeholderRenderer != null)
            {
                originalPlaceholderMaterial = placeholderRenderer.material;
            }
        }
    }
    
    private void SetupPlaceholder()
    {
        if (headPlaceholder != null)
        {
            // Set initial visibility
            headPlaceholder.SetActive(placeholderInitiallyVisible);
            
            // Apply empty material if assigned
            if (placeholderRenderer != null && placeholderEmptyMaterial != null)
            {
                placeholderRenderer.material = placeholderEmptyMaterial;
            }
            
            Debug.Log($"[Puzzle] Placeholder initialized. Visible: {placeholderInitiallyVisible}");
        }
        else
        {
            Debug.LogWarning("[DullahanHeadPlacementPuzzle] No head placeholder assigned!");
        }
    }
    
    private void SetupVisuals()
    {
        // Hide attached head visual initially
        if (attachedHeadVisual != null)
            attachedHeadVisual.SetActive(false);
        
        // Setup completion light
        if (completionLight != null)
            completionLight.enabled = false;
        
        // Hide interaction UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    private void CheckRaycastInteraction()
    {
        if (playerCamera == null) return;
        
        // Raycast from center of screen
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            // Check if we hit this puzzle object or its children
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    OnPlayerEnterRange();
                }
                
                UpdatePlaceholderMaterial();
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    OnPlayerExitRange();
                }
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                OnPlayerExitRange();
            }
        }
    }
    
    private void CheckDistanceInteraction()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
        
        if (playerInRange)
        {
            UpdatePlaceholderMaterial();
        }
    }
    
    private void OnPlayerEnterRange()
    {
        // Show placeholder when player gets close
        if (headPlaceholder != null && !puzzleCompleted)
        {
            headPlaceholder.SetActive(true);
        }
        
        ShowInteractionPrompt();
    }
    
    private void OnPlayerExitRange()
    {
        HideInteractionPrompt();
        
        // Reset placeholder material
        if (placeholderRenderer != null && placeholderEmptyMaterial != null)
        {
            placeholderRenderer.material = placeholderEmptyMaterial;
        }
    }
    
    private void UpdatePlaceholderMaterial()
    {
        if (!playerInRange || placeholderRenderer == null) return;
        
        // Get currently selected head
        DullahanHeadSO currentHead = GetSelectedHead();
        
        if (currentHead == null)
        {
            // No head selected - show empty material
            if (placeholderEmptyMaterial != null)
                placeholderRenderer.material = placeholderEmptyMaterial;
        }
        else if (currentHead.headID == requiredHeadID)
        {
            // Correct head - show valid material
            if (placeholderValidMaterial != null)
                placeholderRenderer.material = placeholderValidMaterial;
        }
        else
        {
            // Wrong head - show invalid material
            if (placeholderInvalidMaterial != null)
                placeholderRenderer.material = placeholderInvalidMaterial;
        }
    }
    
    private void HandleInteractionInput()
    {
        if (!playerInRange || puzzleCompleted) return;
        
        if (Input.GetKeyDown(interactionKey))
        {
            TryPlaceHead();
        }
    }
    
    private void TryPlaceHead()
    {
        if (headInventory == null)
        {
            Debug.LogError("[Puzzle] Head inventory not found!");
            return;
        }
        
        // Check if player has any heads
        if (!headInventory.HasHeads())
        {
            Debug.Log("[Puzzle] Player has no heads to place");
            ShowNoHeadMessage();
            return;
        }
        
        // Get currently selected head
        DullahanHeadSO selectedHead = GetSelectedHead();
        if (selectedHead == null)
        {
            Debug.Log("[Puzzle] No head currently selected");
            ShowNoHeadMessage();
            return;
        }
        
        Debug.Log($"[Puzzle] Attempting to place head: {selectedHead.headName} (ID: {selectedHead.headID})");
        
        // Check if it's the correct head
        if (selectedHead.headID == requiredHeadID)
        {
            PlaceCorrectHead(selectedHead);
        }
        else
        {
            PlaceWrongHead(selectedHead);
        }
    }
    
    private void PlaceCorrectHead(DullahanHeadSO headData)
    {
        Debug.Log($"[Puzzle] ✓ CORRECT HEAD PLACED: {headData.headName}");
        
        // Remove head from inventory
        headInventory.RemoveSelectedHeadIfHead();
        
        // Hide placeholder
        if (headPlaceholder != null)
            headPlaceholder.SetActive(false);
        
        // Show attached head visual
        if (attachedHeadVisual != null)
        {
            attachedHeadVisual.SetActive(true);
            
            // If headData has a prefab, instantiate it at attachment point
            if (headData.headPrefab != null && headAttachmentPoint != null)
            {
                GameObject instantiatedHead = Instantiate(headData.headPrefab, headAttachmentPoint);
                instantiatedHead.transform.localPosition = Vector3.zero;
                instantiatedHead.transform.localRotation = Quaternion.identity;
                
                // Remove any physics components from the instantiated head
                Rigidbody rb = instantiatedHead.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);
                
                Collider col = instantiatedHead.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
        }
        
        // Play effects
        PlaySound(correctHeadSound);
        
        if (placementParticles != null)
            placementParticles.Play();
        
        // Complete puzzle
        CompletePuzzle();
    }
    
    private void PlaceWrongHead(DullahanHeadSO headData)
    {
        Debug.Log($"[Puzzle] ✗ WRONG HEAD PLACED: {headData.headName}");
        
        // Remove head from inventory
        headInventory.RemoveSelectedHeadIfHead();
        
        // Show wrong head briefly if enabled
        if (showFakeHeadBriefly)
        {
            StartCoroutine(ShowFakeHeadTemporarily(headData));
        }
        
        // Play wrong head sound
        PlaySound(wrongHeadSound);
        
        // Show message
        StartCoroutine(ShowTemporaryMessage("That's not the right head!"));
        
        // Apply any negative effects from the wrong head
        ApplyWrongHeadEffects(headData);
    }
    
    private IEnumerator ShowFakeHeadTemporarily(DullahanHeadSO headData)
    {
        if (headData.headPrefab == null || headAttachmentPoint == null) yield break;
        
        // Hide placeholder
        if (headPlaceholder != null)
            headPlaceholder.SetActive(false);
        
        // Instantiate fake head
        currentFakeHeadInstance = Instantiate(headData.headPrefab, headAttachmentPoint);
        currentFakeHeadInstance.transform.localPosition = Vector3.zero;
        currentFakeHeadInstance.transform.localRotation = Quaternion.identity;
        
        // Remove physics
        Rigidbody rb = currentFakeHeadInstance.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        Collider col = currentFakeHeadInstance.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        Debug.Log($"[Puzzle] Showing fake head for {fakeHeadDisplayDuration} seconds");
        
        // Wait
        yield return new WaitForSeconds(fakeHeadDisplayDuration);
        
        // Remove fake head
        if (currentFakeHeadInstance != null)
        {
            Destroy(currentFakeHeadInstance);
            currentFakeHeadInstance = null;
        }
        
        // Show placeholder again
        if (headPlaceholder != null && !puzzleCompleted)
            headPlaceholder.SetActive(true);
        
        Debug.Log("[Puzzle] Fake head removed, placeholder restored");
    }
    
    private void ApplyWrongHeadEffects(DullahanHeadSO headData)
    {
        if (headData == null || !headData.hasEffect) return;
        
        Debug.Log($"[Puzzle] Applying wrong head effects: {headData.effectType}");
        
        // You can implement specific effects here based on the head data
        // For example: slow player, damage player, spawn enemies, etc.
        
        // Find effect manager and apply effects
        DullahanHeadEffectManager effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        if (effectManager != null)
        {
            effectManager.ApplyHeadEffect(headData);
        }
    }
    
    private void CompletePuzzle()
    {
        puzzleCompleted = true;
        
        Debug.Log("[Puzzle] ★★★ PUZZLE COMPLETED! ★★★");
        
        // Unfreeze Dullahan since puzzle is complete
        if (isDullahanFrozen)
        {
            UnfreezeDullahan();
        }
        
        // Play completion sound
        PlaySound(puzzleCompleteSound);
        
        // Show completion particles
        if (completionParticles != null)
            completionParticles.Play();
        
        // Enable completion light
        if (completionLight != null)
        {
            completionLight.enabled = true;
            completionLight.color = completedLightColor;
        }
        
        // Hide interaction UI
        HideInteractionPrompt();
        
        // Grant rewards
        GrantRewards();
        
        // Notify event managers
        NotifyEventManagers();
    }
    
    private void GrantRewards()
    {
        // Unlock door
        if (rewardDoor != null)
        {
            rewardDoor.UnlockDoor();
            Debug.Log("[Puzzle] Reward door unlocked");
        }
        
        // Spawn reward items
        if (rewardItems != null && rewardItems.Length > 0)
        {
            Transform spawnPoint = rewardSpawnPoint != null ? rewardSpawnPoint : transform;
            
            foreach (GameObject rewardItem in rewardItems)
            {
                if (rewardItem != null)
                {
                    Instantiate(rewardItem, spawnPoint.position, spawnPoint.rotation);
                    Debug.Log($"[Puzzle] Spawned reward: {rewardItem.name}");
                }
            }
        }
    }
    
    private void NotifyEventManagers()
    {
        // Notify any event managers about puzzle completion
        Floor2EndingEventManager floor2EventManager = FindObjectOfType<Floor2EndingEventManager>();
        if (floor2EventManager != null)
        {
            floor2EventManager.OnRealHeadAttachedToBody();
            Debug.Log("[Puzzle] Notified Floor2EndingEventManager");
        }
        
        DullahanChaseEventManager chaseEventManager = FindObjectOfType<DullahanChaseEventManager>();
        if (chaseEventManager != null)
        {
            chaseEventManager.OnRealHeadAttachedToBody();
            Debug.Log("[Puzzle] Notified DullahanChaseEventManager");
        }
    }
    
    private void ShowInteractionPrompt()
    {
        if (isShowingPrompt) return;
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            isShowingPrompt = true;
        }
        
        UpdatePromptText();
    }
    
    private void HideInteractionPrompt()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            isShowingPrompt = false;
        }
    }
    
    private void UpdatePromptText()
    {
        if (interactionText == null) return;
        
        DullahanHeadSO currentHead = GetSelectedHead();
        
        if (currentHead == null)
        {
            interactionText.text = noHeadPrompt;
        }
        else
        {
            interactionText.text = interactionPrompt;
        }
    }
    
    private void ShowNoHeadMessage()
    {
        StartCoroutine(ShowTemporaryMessage(noHeadPrompt));
    }
    
    private IEnumerator ShowTemporaryMessage(string message)
    {
        if (interactionText != null)
        {
            string originalText = interactionText.text;
            interactionText.text = message;
            
            yield return new WaitForSeconds(2f);
            
            if (!puzzleCompleted)
                UpdatePromptText();
        }
    }
    
    private DullahanHeadSO GetSelectedHead()
    {
        if (headInventory == null) return null;
        return headInventory.GetCurrentHead();
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // ==========================================
    // Dullahan Freeze/Unfreeze System
    // ==========================================
    
    /// <summary>
    /// Check if player has a head and freeze/unfreeze Dullahan accordingly
    /// </summary>
    private void CheckAndFreezeDullahan()
    {
        if (headInventory == null) return;
        
        bool playerHasHead = headInventory.HasHeads();
        
        // Check if state changed
        if (playerHasHead && !playerPreviouslyHadHead)
        {
            // Player just picked up a head - freeze Dullahan
            FreezeDullahan();
        }
        else if (!playerHasHead && playerPreviouslyHadHead)
        {
            // Player no longer has a head (placed it or dropped it) - unfreeze Dullahan
            UnfreezeDullahan();
        }
        
        playerPreviouslyHadHead = playerHasHead;
    }
    
    /// <summary>
    /// Freeze the Dullahan (stop movement and chase)
    /// </summary>
    private void FreezeDullahan()
    {
        if (isDullahanFrozen) return; // Already frozen
        
        Debug.Log("[Puzzle] 🥶 FREEZING DULLAHAN - Player has picked up a head!");
        
        // Stop chase system if available
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
            Debug.Log("[Puzzle] Dullahan chase ended");
        }
        
        // Stop NavMeshAgent movement
        if (dullahanAgent != null)
        {
            dullahanAgent.isStopped = true;
            dullahanAgent.velocity = Vector3.zero;
            Debug.Log("[Puzzle] Dullahan NavMeshAgent stopped");
        }
        
        isDullahanFrozen = true;
        Debug.Log("[Puzzle] ✓ Dullahan is now frozen. Player can safely place the head!");
    }
    
    /// <summary>
    /// Unfreeze the Dullahan (resume movement and chase)
    /// </summary>
    private void UnfreezeDullahan()
    {
        if (!isDullahanFrozen) return; // Already unfrozen
        
        Debug.Log("[Puzzle] 🔥 UNFREEZING DULLAHAN - Player no longer has a head!");
        
        // Resume NavMeshAgent movement
        if (dullahanAgent != null)
        {
            dullahanAgent.isStopped = false;
            Debug.Log("[Puzzle] Dullahan NavMeshAgent resumed");
        }
        
        // Resume chase/patrol
        if (dullahanChaseSystem != null)
        {
            // Check if puzzle is completed to decide whether to start chase or patrol
            if (puzzleCompleted)
            {
                // Puzzle complete - return to patrol
                dullahanChaseSystem.StartPatrol();
                Debug.Log("[Puzzle] Dullahan returning to patrol (puzzle complete)");
            }
            else
            {
                // Puzzle not complete - resume chase
                dullahanChaseSystem.StartChase();
                Debug.Log("[Puzzle] Dullahan resuming chase");
            }
        }
        
        isDullahanFrozen = false;
        Debug.Log("[Puzzle] ✓ Dullahan is now unfrozen and can move again!");
    }
    
    /// <summary>
    /// Manually freeze the Dullahan (for testing or external control)
    /// </summary>
    public void ManuallyFreezeDullahan()
    {
        FreezeDullahan();
    }
    
    /// <summary>
    /// Manually unfreeze the Dullahan (for testing or external control)
    /// </summary>
    public void ManuallyUnfreezeDullahan()
    {
        UnfreezeDullahan();
    }
    
    /// <summary>
    /// Check if Dullahan is currently frozen
    /// </summary>
    public bool IsDullahanFrozen()
    {
        return isDullahanFrozen;
    }
    
    // Public methods for external scripts
    
    /// <summary>
    /// Check if the puzzle is completed
    /// </summary>
    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
    
    /// <summary>
    /// Reset the puzzle to its initial state
    /// </summary>
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        
        if (headPlaceholder != null)
            headPlaceholder.SetActive(placeholderInitiallyVisible);
        
        if (attachedHeadVisual != null)
            attachedHeadVisual.SetActive(false);
        
        if (completionLight != null)
            completionLight.enabled = false;
        
        if (currentFakeHeadInstance != null)
        {
            Destroy(currentFakeHeadInstance);
            currentFakeHeadInstance = null;
        }
        
        // Unfreeze Dullahan if frozen
        if (isDullahanFrozen)
        {
            UnfreezeDullahan();
        }
        
        // Reset state tracking
        playerPreviouslyHadHead = false;
        
        Debug.Log("[Puzzle] Puzzle reset");
    }
    
    /// <summary>
    /// Force complete the puzzle (for testing)
    /// </summary>
    public void ForceComplete()
    {
        CompletePuzzle();
    }
    
    // Gizmos for editor visualization
    void OnDrawGizmos()
    {
        // Draw interaction range
        Gizmos.color = puzzleCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to attachment point
        if (headAttachmentPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, headAttachmentPoint.position);
            Gizmos.DrawWireSphere(headAttachmentPoint.position, 0.1f);
        }
    }
}


using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 🏛️ HEAD SHRINE PUZZLE - Single Altar with Hidden Heads!
/// 
/// A mystical shrine with a single altar built from different assets (table, candles, etc.).
/// The map contains 3 hidden heads that players must find and place on the altar.
/// - 2 Wrong Heads: One opens a door, one has no reward
/// - 1 Real Head: Spawns a key or opens the main door
/// 
/// SETUP (3 steps):
/// 1. Create empty GameObject with this script
/// 2. Create child objects for placement points (name them "Placement1", "Placement2", etc.)
/// 3. Assign head IDs, rewards, and materials in inspector
/// 
/// THAT'S IT! No complex setup needed.
/// </summary>
public class HeadShrinePuzzle : MonoBehaviour
{
    [Header("🏛️ Shrine Settings")]
    [Tooltip("List of head IDs that need to be placed on placement points (in order)")]
    public int[] requiredHeadIDs = { 1, 2, 3 }; // Example: Real head, Wrong head 1, Wrong head 2
    
    [Tooltip("Which placement point requires the REAL head (0 = first, 1 = second, etc.)")]
    public int realHeadPlacementIndex = 0; // Center placement point
    
    [Tooltip("How close player needs to be to interact with any placement point")]
    public float interactionDistance = 4f;
    
    [Tooltip("Key to press for interacting with the shrine (default: F)")]
    public KeyCode interactionKey = KeyCode.F;
    
    [Header("🏗️ Altar Assets (Assign Existing Objects)")]
    [Tooltip("The main altar/table GameObject (e.g., TableV2)")]
    public GameObject altarBase;
    
    [Tooltip("Left candle holder GameObject (e.g., CandleV1)")]
    public GameObject leftCandle;
    
    [Tooltip("Center candle holder GameObject (e.g., CandleV2)")]
    public GameObject centerCandle;
    
    [Tooltip("Right candle holder GameObject (e.g., CandleV3)")]
    public GameObject rightCandle;
    
    [Header("🎯 Head Rewards")]
    [Tooltip("Door that opens when the first wrong head is placed")]
    public doorscript wrongHead1Door;
    
    [Tooltip("Door that opens when the second wrong head is placed")]
    public doorscript wrongHead2Door;
    
    [Tooltip("Main door that opens when the real head is placed")]
    public doorscript realHeadDoor;
    
    [Tooltip("Key that spawns when the real head is placed")]
    public GameObject keyReward;
    
    [Tooltip("Transform where the key spawns")]
    public Transform keySpawnPoint;
    
    [Header("🎨 Visual Settings")]
    [Tooltip("Material for empty placement points")]
    public Material emptyPlacementMaterial;
    
    [Tooltip("Material for placement points with heads")]
    public Material filledPlacementMaterial;
    
    [Tooltip("Material for the real head placement (center)")]
    public Material realHeadPlacementMaterial;
    
    [Tooltip("Material for the altar base")]
    public Material altarBaseMaterial;
    
    [Header("🔥 Fire Effects")]
    [Tooltip("Particle system for placement point activation")]
    public GameObject activationParticlePrefab;
    
    [Tooltip("Light component for placement point glow")]
    public Light placementLightPrefab;
    
    [Header("🎵 Audio")]
    public AudioClip headPlacedSound;
    public AudioClip placementActivatedSound;
    public AudioClip shrineCompleteSound;
    public AudioClip wrongHeadSound;
    public AudioClip realHeadPlacedSound;
    public AudioClip doorOpenSound;
    public AudioClip keySpawnSound;
    public AudioClip mysticalChantingSound;
    
    [Header("💬 Placement Prompt")]
    [Tooltip("UI GameObject that shows placement prompt")]
    public GameObject placementPromptUI;
    
    [Tooltip("Text component for placement prompt")]
    public TMPro.TextMeshProUGUI placementPromptText;
    
    [Tooltip("Text to show when player can place head (use {0} for key placeholder)")]
    public string placeHeadText = "Press {0} to place head";
    
    [Tooltip("Text to show when no head in inventory")]
    public string noHeadText = "No head in inventory";
    
    [Tooltip("Text to show when all placements are full")]
    public string allFullText = "All placements are full";
    
    [Header("🎁 General Rewards")]
    public doorscript rewardDoor;
    public GameObject[] rewardItems;
    public Transform rewardSpawnPoint;
    
    [Header("✨ Shrine Activation")]
    [Tooltip("GameObject to activate when shrine is complete")]
    public GameObject shrineActivationEffect;
    
    [Tooltip("Animation for shrine completion")]
    public Animator shrineAnimator;
    
    // Private variables
    private Transform player;
    private DullahanHeadInventory inventory;
    private AudioSource audioSource;
    private List<ShrinePlacement> placements = new List<ShrinePlacement>();
    private bool shrineComplete = false;
    private bool isPlayingChanting = false;
    private bool playerInRange = false;
    private ShrinePlacement currentPlacement = null;
    
    // Shrine placement class
    [System.Serializable]
    public class ShrinePlacement
    {
        public Transform placementTransform;
        public Renderer placementRenderer;
        public Light placementLight;
        public ParticleSystem activationParticles;
        public int requiredHeadID;
        public bool isActivated = false;
        public bool hasHead = false;
        public DullahanHeadSO placedHead;
        public GameObject spawnedHeadModel;
        public bool isRealHeadPlacement = false;
        
        public ShrinePlacement(Transform transform, int headID, bool isRealHead)
        {
            placementTransform = transform;
            requiredHeadID = headID;
            isRealHeadPlacement = isRealHead;
            placementRenderer = transform.GetComponent<Renderer>();
            placementLight = transform.GetComponent<Light>();
            activationParticles = transform.GetComponent<ParticleSystem>();
        }
    }
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        
        // Find inventory
        inventory = FindObjectOfType<DullahanHeadInventory>();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Setup placements automatically
        SetupPlacements();
        
        // Start mystical chanting
        StartCoroutine(PlayMysticalChanting());
        
        Debug.Log($"[HeadShrinePuzzle] Shrine setup complete! {placements.Count} placements created.");
    }
    
    void SetupPlacements()
    {
        // Clear existing placements
        placements.Clear();
        
        // First, try to find child placement points
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Check if this looks like a placement point
            if (child.name.ToLower().Contains("placement") || 
                child.name.StartsWith("Placement"))
            {
                int headID = (placements.Count < requiredHeadIDs.Length) ? requiredHeadIDs[placements.Count] : 0;
                bool isRealHead = (placements.Count == realHeadPlacementIndex);
                ShrinePlacement placement = new ShrinePlacement(child, headID, isRealHead);
                placements.Add(placement);
                
                // Setup placement appearance
                SetupPlacementAppearance(placement);
                
                Debug.Log($"[HeadShrinePuzzle] Found child placement: {child.name} for head ID {headID} (Real head: {isRealHead})");
            }
        }
        
        // If no child placements found, create them automatically based on altar assets
        if (placements.Count == 0)
        {
            CreatePlacementsFromAltarAssets();
        }
    }
    
    void CreatePlacementsFromAltarAssets()
    {
        Debug.Log("[HeadShrinePuzzle] Creating placements based on existing altar assets...");
        
        // Get the altar base position for reference
        Vector3 altarPosition = Vector3.zero;
        if (altarBase)
        {
            altarPosition = altarBase.transform.position;
        }
        
        // Create placement points based on candle positions
        for (int i = 0; i < requiredHeadIDs.Length; i++)
        {
            // Create placement point
            GameObject placementObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placementObj.name = $"Placement{i + 1}";
            placementObj.transform.SetParent(transform);
            
            // Position placements based on candle positions
            Vector3 placementPosition = altarPosition;
            
            if (i == 0) // Center (real head) - use center candle position
            {
                if (centerCandle)
                {
                    placementPosition = centerCandle.transform.position;
                    placementPosition.y += 0.5f; // Slightly above candle
                }
                else
                {
                    placementPosition.y += 0.2f;
                }
            }
            else if (i == 1) // Left - use left candle position
            {
                if (leftCandle)
                {
                    placementPosition = leftCandle.transform.position;
                    placementPosition.y += 0.5f; // Slightly above candle
                }
                else
                {
                    placementPosition.x -= 1.5f;
                    placementPosition.y += 0.2f;
                }
            }
            else // Right - use right candle position
            {
                if (rightCandle)
                {
                    placementPosition = rightCandle.transform.position;
                    placementPosition.y += 0.5f; // Slightly above candle
                }
                else
                {
                    placementPosition.x += 1.5f;
                    placementPosition.y += 0.2f;
                }
            }
            
            placementObj.transform.position = placementPosition;
            placementObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            
            // Remove collider
            Destroy(placementObj.GetComponent<Collider>());
            
            bool isRealHead = (i == realHeadPlacementIndex);
            ShrinePlacement placement = new ShrinePlacement(placementObj.transform, requiredHeadIDs[i], isRealHead);
            placements.Add(placement);
            
            // Setup appearance
            SetupPlacementAppearance(placement);
            
            Debug.Log($"[HeadShrinePuzzle] Created placement {i + 1} at position: {placementPosition} (Real head: {isRealHead})");
        }
    }
    
    void SetupPlacementAppearance(ShrinePlacement placement)
    {
        // Set placement material based on type
        if (placement.placementRenderer)
        {
            if (placement.isRealHeadPlacement && realHeadPlacementMaterial)
            {
                placement.placementRenderer.material = realHeadPlacementMaterial;
            }
            else if (emptyPlacementMaterial)
            {
                placement.placementRenderer.material = emptyPlacementMaterial;
            }
        }
        
        // Setup placement light
        if (placement.placementLight == null && placementLightPrefab)
        {
            placement.placementLight = Instantiate(placementLightPrefab, placement.placementTransform);
            placement.placementLight.enabled = false;
        }
        
        // Setup activation particles
        if (placement.activationParticles == null && activationParticlePrefab)
        {
            GameObject particleObj = Instantiate(activationParticlePrefab, placement.placementTransform);
            placement.activationParticles = particleObj.GetComponent<ParticleSystem>();
            if (placement.activationParticles) placement.activationParticles.Stop();
        }
    }
    
    void Update()
    {
        if (shrineComplete || !player || !inventory) return;
        
        // Check if player is close enough to any placement point
        bool wasInRange = playerInRange;
        playerInRange = false;
        currentPlacement = null;
        
        foreach (ShrinePlacement placement in placements)
        {
            float distance = Vector3.Distance(placement.placementTransform.position, player.position);
            if (distance <= interactionDistance)
            {
                playerInRange = true;
                currentPlacement = placement;
                
                // Check if player has a head and presses the interaction key
                if (Input.GetKeyDown(interactionKey))
                {
                    TryPlaceHeadOnPlacement(placement);
                }
                break; // Only check one placement at a time
            }
        }
        
        // Update prompt visibility
        UpdatePlacementPrompt();
    }
    
    void UpdatePlacementPrompt()
    {
        if (placementPromptUI == null) return;
        
        if (playerInRange && currentPlacement != null)
        {
            placementPromptUI.SetActive(true);
            
            if (placementPromptText != null)
            {
                // Check if player has a head
                DullahanHeadSO currentHead = inventory.GetCurrentHead();
                if (currentHead == null)
                {
                    placementPromptText.text = noHeadText;
                }
                else if (currentPlacement.hasHead)
                {
                    placementPromptText.text = allFullText;
                }
                else
                {
                    placementPromptText.text = string.Format(placeHeadText, interactionKey.ToString());
                }
            }
        }
        else
        {
            placementPromptUI.SetActive(false);
        }
    }
    
    void TryPlaceHeadOnPlacement(ShrinePlacement placement)
    {
        // Get current head from inventory
        DullahanHeadSO currentHead = inventory.GetCurrentHead();
        if (currentHead == null)
        {
            Debug.Log("[HeadShrinePuzzle] No head in inventory - no interaction");
            return; // Do nothing if no head
        }
        
        Debug.Log($"[HeadShrinePuzzle] Trying to place head: {currentHead.headName} (ID: {currentHead.headID}) on placement");
        
        // Check if this is the correct head for this placement
        if (currentHead.headID == placement.requiredHeadID && !placement.hasHead)
        {
            PlaceHeadOnPlacement(currentHead, placement);
        }
        else
        {
            // Wrong head or placement already has a head
            HandleWrongHead(currentHead);
        }
    }
    
    void PlaceHeadOnPlacement(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] ✓ Placing {head.headName} on placement!");
        
        // Remove from inventory (like pickup but in reverse)
        inventory.RemoveSelectedHeadIfHead();
        
        // Mark placement as having a head
        placement.hasHead = true;
        placement.placedHead = head; // Store the head reference
        
        // Create head object on shrine (like pickup but in reverse)
        CreateHeadObjectOnShrine(head, placement);
        
        // Update placement material
        if (placement.placementRenderer && filledPlacementMaterial)
        {
            placement.placementRenderer.material = filledPlacementMaterial;
        }
        
        // Activate the placement
        ActivatePlacement(placement);
        
        // Handle rewards based on head type
        HandleHeadRewards(head, placement);
        
        // Play placement sound
        if (placement.isRealHeadPlacement && realHeadPlacedSound)
        {
            audioSource.PlayOneShot(realHeadPlacedSound);
        }
        else if (headPlacedSound)
        {
            audioSource.PlayOneShot(headPlacedSound);
        }
        
        // Check if shrine is complete
        CheckShrineCompletion();
    }
    
    void ActivatePlacement(ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] ✨ Activating placement!");
        
        placement.isActivated = true;
        
        // Enable placement light
        if (placement.placementLight)
        {
            placement.placementLight.enabled = true;
        }
        
        // Start activation particles
        if (placement.activationParticles)
        {
            placement.activationParticles.Play();
        }
        
        // Play activation sound
        if (placementActivatedSound) audioSource.PlayOneShot(placementActivatedSound);
    }
    
    void HandleHeadRewards(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] 🎁 Handling rewards for {head.headName} (ID: {head.headID})!");
        Debug.Log($"[HeadShrinePuzzle] Is real head placement: {placement.isRealHeadPlacement}");
        
        // Check if this is the real head
        if (placement.isRealHeadPlacement)
        {
            Debug.Log($"[HeadShrinePuzzle] 🗝️ This is a REAL HEAD placement!");
            HandleRealHeadRewards();
        }
        else
        {
            Debug.Log($"[HeadShrinePuzzle] 🚪 This is a WRONG HEAD placement!");
            HandleWrongHeadRewards(head, placement);
        }
    }
    
    void HandleRealHeadRewards()
    {
        Debug.Log($"[HeadShrinePuzzle] 🗝️ REAL HEAD PLACED! Granting main rewards!");
        
        // Open main door
        if (realHeadDoor)
        {
            realHeadDoor.UnlockDoor();
            realHeadDoor.OpenDoor();
            if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);
            Debug.Log("[HeadShrinePuzzle] Main door unlocked and opened!");
        }
        
        // Spawn key
        if (keyReward && keySpawnPoint)
        {
            Instantiate(keyReward, keySpawnPoint.position, keySpawnPoint.rotation);
            if (keySpawnSound) audioSource.PlayOneShot(keySpawnSound);
            Debug.Log("[HeadShrinePuzzle] Key spawned!");
        }
        
        // Notify event managers
        Floor2EndingEventManager eventManager = FindObjectOfType<Floor2EndingEventManager>();
        if (eventManager) eventManager.OnRealHeadAttached();
    }
    
    void HandleWrongHeadRewards(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] 🚪 Wrong head placed: {head.headName} (ID: {head.headID})");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 1 door assigned: {wrongHead1Door != null}");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 2 door assigned: {wrongHead2Door != null}");
        
        // Determine which wrong head this is based on head ID
        // Assuming wrong heads have IDs 2 and 3 (adjust based on your setup)
        if (head.headID == 2) // First wrong head
        {
            Debug.Log($"[HeadShrinePuzzle] 🚪 Processing wrong head 1 (ID: 2)");
            if (wrongHead1Door)
            {
                Debug.Log($"[HeadShrinePuzzle] Unlocking and opening wrong head 1 door...");
                wrongHead1Door.UnlockDoor();
                wrongHead1Door.OpenDoor();
                if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);
                Debug.Log("[HeadShrinePuzzle] ✅ Wrong head 1 door unlocked and opened!");
            }
            else
            {
                Debug.LogWarning("[HeadShrinePuzzle] ❌ Wrong head 1 door not assigned!");
            }
        }
        else if (head.headID == 3) // Second wrong head
        {
            Debug.Log($"[HeadShrinePuzzle] 🚪 Processing wrong head 2 (ID: 3)");
            if (wrongHead2Door)
            {
                Debug.Log($"[HeadShrinePuzzle] Unlocking and opening wrong head 2 door...");
                wrongHead2Door.UnlockDoor();
                wrongHead2Door.OpenDoor();
                if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);
                Debug.Log("[HeadShrinePuzzle] ✅ Wrong head 2 door unlocked and opened!");
            }
            else
            {
                Debug.LogWarning("[HeadShrinePuzzle] ❌ Wrong head 2 door not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ❌ Unknown wrong head ID: {head.headID}. Expected 2 or 3.");
        }
    }
    
    void HandleWrongHead(DullahanHeadSO head)
    {
        Debug.Log($"[HeadShrinePuzzle] ✗ Wrong head: {head.headName}");
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Play wrong sound
        if (wrongHeadSound) audioSource.PlayOneShot(wrongHeadSound);
        
        // Could add visual feedback here (screen flash, etc.)
    }
    
    void CheckShrineCompletion()
    {
        // Check if all placements are activated
        bool allActivated = true;
        foreach (ShrinePlacement placement in placements)
        {
            if (!placement.isActivated)
            {
                allActivated = false;
                break;
            }
        }
        
        if (allActivated)
        {
            CompleteShrine();
        }
    }
    
    void CompleteShrine()
    {
        Debug.Log("[HeadShrinePuzzle] 🏛️ SHRINE COMPLETED!");
        
        shrineComplete = true;
        
        // Stop chanting
        StopMysticalChanting();
        
        // Play completion sound
        if (shrineCompleteSound) audioSource.PlayOneShot(shrineCompleteSound);
        
        // Activate shrine effects
        if (shrineActivationEffect) shrineActivationEffect.SetActive(true);
        
        // Play shrine animation
        if (shrineAnimator) shrineAnimator.SetTrigger("Complete");
        
        // Grant general rewards (if any)
        if (rewardDoor) 
        {
            rewardDoor.UnlockDoor();
            rewardDoor.OpenDoor();
        }
        
        if (rewardItems != null && rewardSpawnPoint != null)
        {
            foreach (GameObject item in rewardItems)
            {
                if (item) Instantiate(item, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
            }
        }
        
        Debug.Log("[HeadShrinePuzzle] Shrine completion effects activated!");
    }
    
    IEnumerator PlayMysticalChanting()
    {
        while (!shrineComplete && mysticalChantingSound)
        {
            if (!isPlayingChanting)
            {
                isPlayingChanting = true;
                audioSource.PlayOneShot(mysticalChantingSound);
                yield return new WaitForSeconds(mysticalChantingSound.length);
                isPlayingChanting = false;
            }
            yield return new WaitForSeconds(5f); // Wait before next chant
        }
    }
    
    void StopMysticalChanting()
    {
        isPlayingChanting = false;
        if (audioSource.isPlaying) audioSource.Stop();
    }
    
    void CleanupHeadComponents(GameObject headObj)
    {
        // Remove all interactive components
        Rigidbody[] rbs = headObj.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs) if (rb) Destroy(rb);
        
        Collider[] cols = headObj.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols) if (col) Destroy(col);
        
        DullahanHeadPickable[] pickables = headObj.GetComponentsInChildren<DullahanHeadPickable>(true);
        foreach (var pickable in pickables) if (pickable) Destroy(pickable);
    }
    
    void CreateHeadObjectOnShrine(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] 🎭 Creating head object on shrine: {head.headName}");
        
        // Create head object from prefab (like pickup but in reverse)
        if (head.headPrefab)
        {
            GameObject headObject = Instantiate(head.headPrefab, placement.placementTransform);
            headObject.transform.localPosition = new Vector3(0, 0.3f, 0);
            headObject.transform.localRotation = Quaternion.identity;
            headObject.transform.localScale = Vector3.one * 0.8f;
            
            // Store the created head object
            placement.spawnedHeadModel = headObject;
            
            // Setup head object like pickup system but in reverse
            SetupHeadObjectOnShrine(headObject, head);
            
            Debug.Log($"[HeadShrinePuzzle] ✅ Head object created successfully on shrine!");
        }
        else
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ No head prefab assigned for {head.headName}");
        }
    }
    
    void SetupHeadObjectOnShrine(GameObject headObject, DullahanHeadSO headData)
    {
        // Add DullahanHeadPickable component (like pickup system)
        DullahanHeadPickable headPickable = headObject.GetComponent<DullahanHeadPickable>();
        if (headPickable == null)
        {
            headPickable = headObject.AddComponent<DullahanHeadPickable>();
        }
        
        // Setup head data
        headPickable.headData = headData;
        headPickable.isPickedUp = true; // Mark as already "picked up" (placed)
        
        // Setup visual effects (like pickup system)
        SetupHeadVisualEffects(headObject, headData);
        
        // Remove interactive components so it can't be picked up again
        CleanupHeadComponents(headObject);
        
        Debug.Log($"[HeadShrinePuzzle] 🎨 Head object setup complete!");
    }
    
    void SetupHeadVisualEffects(GameObject headObject, DullahanHeadSO headData)
    {
        // Setup glow effect (like pickup system)
        if (headData.hasGlowEffect)
        {
            Light headLight = headObject.GetComponent<Light>();
            if (headLight == null)
            {
                headLight = headObject.AddComponent<Light>();
            }
            
            headLight.color = headData.headGlowColor;
            headLight.intensity = 1f;
            headLight.range = 3f;
        }
        
        // Setup material (like pickup system)
        Renderer headRenderer = headObject.GetComponent<Renderer>();
        if (headRenderer != null && headData.headMaterial != null)
        {
            headRenderer.material = headData.headMaterial;
        }
        
        // Setup audio (like pickup system)
        AudioSource headAudio = headObject.GetComponent<AudioSource>();
        if (headAudio == null)
        {
            headAudio = headObject.AddComponent<AudioSource>();
        }
        
        // Play placement sound if available
        if (headData.pickupSound != null)
        {
            headAudio.PlayOneShot(headData.pickupSound);
        }
    }
    
    // Public methods for other scripts
    public bool IsShrineComplete() => shrineComplete;
    
    public int GetActivatedPlacementsCount()
    {
        int count = 0;
        foreach (ShrinePlacement placement in placements)
        {
            if (placement.isActivated) count++;
        }
        return count;
    }
    
    public int GetTotalPlacementsCount() => placements.Count;
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Draw altar assets
        if (altarBase)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(altarBase.transform.position, Vector3.one * 0.5f);
        }
        
        if (leftCandle)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(leftCandle.transform.position, Vector3.one * 0.3f);
        }
        
        if (centerCandle)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centerCandle.transform.position, Vector3.one * 0.3f);
        }
        
        if (rightCandle)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(rightCandle.transform.position, Vector3.one * 0.3f);
        }
        
        // Draw placement positions
        for (int i = 0; i < placements.Count; i++)
        {
            if (placements[i].placementTransform)
            {
                Gizmos.color = placements[i].isRealHeadPlacement ? Color.red : Color.yellow;
                Gizmos.DrawWireCube(placements[i].placementTransform.position, Vector3.one * 0.3f);
            }
        }
    }
}

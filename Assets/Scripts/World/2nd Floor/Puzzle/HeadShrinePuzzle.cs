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

    [Tooltip("Which placement point requires the REAL head (0 = Left, 1 = Right, 2 = Center)")]
    public int realHeadPlacementIndex = 2; // Center placement (last in sequence)
    
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
        public int placementIndex; // Index in the placements list (0, 1, 2, etc.)
        public bool isActivated = false;
        public bool hasHead = false;
        public DullahanHeadSO placedHead;
        public GameObject spawnedHeadModel;
        public bool isRealHeadPlacement = false;

        public ShrinePlacement(Transform transform, int headID, bool isRealHead, int index)
        {
            placementTransform = transform;
            requiredHeadID = headID;
            isRealHeadPlacement = isRealHead;
            placementIndex = index;
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

        // Validation: Show configuration summary
        Debug.Log($"[HeadShrinePuzzle] ═══════════════════════════════════");
        Debug.Log($"[HeadShrinePuzzle] Shrine setup complete! {placements.Count} placements created.");
        Debug.Log($"[HeadShrinePuzzle] NOTE: Any head can be placed on any placement spot.");
        Debug.Log($"[HeadShrinePuzzle] Real head placement index: {realHeadPlacementIndex}");

        for (int i = 0; i < placements.Count; i++)
        {
            string placementType = placements[i].isRealHeadPlacement ? "REAL HEAD" : "Wrong Head";
            Debug.Log($"[HeadShrinePuzzle] Placement {i}: Type = {placementType} (accepts any head)");
        }

        // Validation warnings
        if (requiredHeadIDs.Length != placements.Count)
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ Mismatch! requiredHeadIDs has {requiredHeadIDs.Length} entries but {placements.Count} placements exist!");
        }

        if (realHeadPlacementIndex >= placements.Count)
        {
            Debug.LogError($"[HeadShrinePuzzle] ❌ realHeadPlacementIndex ({realHeadPlacementIndex}) is out of range! Max index is {placements.Count - 1}");
        }

        // Validate door assignments
        Debug.Log($"[HeadShrinePuzzle] --- Reward Configuration ---");
        Debug.Log($"[HeadShrinePuzzle] Real head door assigned: {realHeadDoor != null}");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 1 door assigned: {wrongHead1Door != null}");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 2 door assigned: {wrongHead2Door != null}");
        Debug.Log($"[HeadShrinePuzzle] Key reward assigned: {keyReward != null}");
        Debug.Log($"[HeadShrinePuzzle] Key spawn point assigned: {keySpawnPoint != null}");

        // Count how many wrong head placements exist
        int wrongHeadCount = 0;
        foreach (var placement in placements)
        {
            if (!placement.isRealHeadPlacement) wrongHeadCount++;
        }

        // Warn if door count doesn't match wrong head count
        if (wrongHeadCount >= 1 && wrongHead1Door == null)
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ You have {wrongHeadCount} wrong head placement(s) but wrongHead1Door is not assigned!");
        }
        if (wrongHeadCount >= 2 && wrongHead2Door == null)
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ You have {wrongHeadCount} wrong head placement(s) but wrongHead2Door is not assigned!");
        }

        Debug.Log($"[HeadShrinePuzzle] ═══════════════════════════════════");
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
                int placementIndex = placements.Count;
                int headID = (placementIndex < requiredHeadIDs.Length) ? requiredHeadIDs[placementIndex] : 0;
                bool isRealHead = (placementIndex == realHeadPlacementIndex);
                ShrinePlacement placement = new ShrinePlacement(child, headID, isRealHead, placementIndex);
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
            // Sequential order: Left (0) → Right (1) → Center (2)
            Vector3 placementPosition = altarPosition;

            if (i == 0) // Left placement - use left candle position
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
            else if (i == 1) // Right placement - use right candle position
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
            else // Center placement (real head) - use center candle position
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
            
            placementObj.transform.position = placementPosition;
            placementObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            
            // Remove collider
            Destroy(placementObj.GetComponent<Collider>());

            bool isRealHead = (i == realHeadPlacementIndex);
            ShrinePlacement placement = new ShrinePlacement(placementObj.transform, requiredHeadIDs[i], isRealHead, i);
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

        // Sequential placement logic: Only allow interaction with the NEXT empty placement
        bool wasInRange = playerInRange;
        playerInRange = false;
        currentPlacement = null;

        // Get the next empty placement in sequence (Left → Right → Center)
        ShrinePlacement nextEmptyPlacement = GetNextEmptyPlacement();

        if (nextEmptyPlacement != null)
        {
            // Check if player is near the NEXT empty placement
            float distanceToNext = Vector3.Distance(nextEmptyPlacement.placementTransform.position, player.position);

            if (distanceToNext <= interactionDistance)
            {
                playerInRange = true;
                currentPlacement = nextEmptyPlacement;

                // Check if player has a head and presses the interaction key
                if (Input.GetKeyDown(interactionKey))
                {
                    TryPlaceHeadOnPlacement(nextEmptyPlacement);
                }
            }
            else
            {
                // Check if player is near any OTHER placement (to show guidance UI)
                foreach (ShrinePlacement placement in placements)
                {
                    if (placement != nextEmptyPlacement)
                    {
                        float distance = Vector3.Distance(placement.placementTransform.position, player.position);
                        if (distance <= interactionDistance)
                        {
                            playerInRange = true;
                            currentPlacement = placement; // This is NOT the correct placement
                            break;
                        }
                    }
                }
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
                else if (AreAllPlacementsFull())
                {
                    placementPromptText.text = allFullText;
                }
                else
                {
                    // Check if player is at the CORRECT (next empty) placement
                    ShrinePlacement nextEmpty = GetNextEmptyPlacement();

                    if (currentPlacement == nextEmpty)
                    {
                        // Player is at the correct placement - allow interaction
                        placementPromptText.text = string.Format(placeHeadText, interactionKey.ToString());
                    }
                    else
                    {
                        // Player is at the wrong placement - guide them to the correct one
                        string directionName = GetPlacementDirectionName(nextEmpty.placementIndex);
                        placementPromptText.text = $"Place at {directionName} first";
                    }
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

        Debug.Log($"[HeadShrinePuzzle] ═══════════════════════════════════");
        Debug.Log($"[HeadShrinePuzzle] Attempting to place head: {currentHead.headName}");
        Debug.Log($"[HeadShrinePuzzle] Current head ID: {currentHead.headID}");
        Debug.Log($"[HeadShrinePuzzle] Placement already has head: {placement.hasHead}");
        Debug.Log($"[HeadShrinePuzzle] Is real head placement: {placement.isRealHeadPlacement}");

        // Check if placement already has a head
        if (placement.hasHead)
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ✗ This placement already has a head! Try another placement.");
            return; // Don't remove head, let player try another spot
        }

        // Allow any head to be placed on any empty placement
        // Rewards are determined by the placement type (real head placement vs wrong head placement)
        Debug.Log($"[HeadShrinePuzzle] ✓ Placement is empty! Placing head...");
        PlaceHeadOnPlacement(currentHead, placement);

        Debug.Log($"[HeadShrinePuzzle] ═══════════════════════════════════");
    }
    
    void PlaceHeadOnPlacement(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] ✓ Placing {head.headName} on placement!");

        // Remove from inventory - find and remove the specific head by ID, not just the selected one
        bool headRemoved = RemoveHeadFromInventory(head);
        if (!headRemoved)
        {
            Debug.LogError($"[HeadShrinePuzzle] Failed to remove head {head.headName} (ID: {head.headID}) from inventory!");
            return;
        }
        Debug.Log($"[HeadShrinePuzzle] Successfully removed head {head.headName} from inventory");

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

    bool RemoveHeadFromInventory(DullahanHeadSO headToRemove)
    {
        // Find the inventory slot containing this specific head
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            var slot = inventory.inventorySlots[i];
            if (slot.isOccupied &&
                slot.itemType == DullahanHeadInventory.InventorySlot.ItemType.Head &&
                slot.headItem != null &&
                slot.headItem.headID == headToRemove.headID)
            {
                Debug.Log($"[HeadShrinePuzzle] Found head {headToRemove.headName} in slot {i}, removing it...");
                inventory.RemoveItemFromSlot(i);
                return true;
            }
        }
        Debug.LogWarning($"[HeadShrinePuzzle] Could not find head {headToRemove.headName} (ID: {headToRemove.headID}) in inventory!");
        return false;
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

        // Open main door - using Force methods for immediate, guaranteed opening
        if (realHeadDoor && realHeadDoor.gameObject != null)
        {
            Debug.Log($"[HeadShrinePuzzle] Force unlocking and opening real head door: {realHeadDoor.gameObject.name}");
            Debug.Log($"[HeadShrinePuzzle] Door state before: isLocked={realHeadDoor.IsLocked()}, isOpen={realHeadDoor.IsOpen()}");

            realHeadDoor.ForceUnlock();  // Immediate unlock without coroutines
            realHeadDoor.ForceOpen();    // Immediate open without animation

            if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);

            Debug.Log($"[HeadShrinePuzzle] Door state after: isLocked={realHeadDoor.IsLocked()}, isOpen={realHeadDoor.IsOpen()}");
            Debug.Log("[HeadShrinePuzzle] ✅ Real head door force unlocked and opened!");
        }
        else
        {
            Debug.LogWarning("[HeadShrinePuzzle] ⚠️ Real head door not assigned or GameObject is null! No door will open.");
        }

        // Spawn or activate key
        if (keyReward)
        {
            // Check if keyReward is a scene object or a prefab
            // Scene objects have a valid scene, prefab assets do not
            if (keyReward.scene.IsValid())
            {
                // It's a scene object - just activate it
                Debug.Log($"[HeadShrinePuzzle] Activating existing key in scene: {keyReward.name}");
                keyReward.SetActive(true);

                // Move to spawn point if provided
                if (keySpawnPoint)
                {
                    keyReward.transform.position = keySpawnPoint.position;
                    keyReward.transform.rotation = keySpawnPoint.rotation;
                    Debug.Log($"[HeadShrinePuzzle] Moved key to spawn point: {keySpawnPoint.position}");
                }

                if (keySpawnSound) audioSource.PlayOneShot(keySpawnSound);
                Debug.Log("[HeadShrinePuzzle] ✅ Key activated and made visible!");
            }
            else
            {
                // It's a prefab - instantiate it
                if (keySpawnPoint)
                {
                    Debug.Log($"[HeadShrinePuzzle] Instantiating key prefab at spawn point: {keySpawnPoint.position}");
                    GameObject spawnedKey = Instantiate(keyReward, keySpawnPoint.position, keySpawnPoint.rotation);
                    spawnedKey.SetActive(true);
                    if (keySpawnSound) audioSource.PlayOneShot(keySpawnSound);
                    Debug.Log($"[HeadShrinePuzzle] ✅ Key instantiated from prefab: {spawnedKey.name}");
                }
                else
                {
                    Debug.LogWarning("[HeadShrinePuzzle] ⚠️ Key reward is a prefab but no spawn point assigned! Key will not spawn.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[HeadShrinePuzzle] ⚠️ Key reward not assigned!");
        }

        // NOTE: Floor2EndingEventManager is no longer needed - functionality moved to DullahanChaseEventManager
        // Event manager notification removed as it's no longer required for the simplified chase system
        // The chase cycle now operates independently based on proximity detection
    }
    
    void HandleWrongHeadRewards(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] 🚪 Wrong head placed: {head.headName} (ID: {head.headID})");
        Debug.Log($"[HeadShrinePuzzle] Placement index: {placement.placementIndex}");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 1 door assigned: {wrongHead1Door != null}");
        Debug.Log($"[HeadShrinePuzzle] Wrong head 2 door assigned: {wrongHead2Door != null}");

        // Find all wrong head placements (non-real-head placements) and sort by index
        List<int> wrongHeadIndices = new List<int>();
        for (int i = 0; i < placements.Count; i++)
        {
            if (!placements[i].isRealHeadPlacement)
            {
                wrongHeadIndices.Add(placements[i].placementIndex);
            }
        }
        wrongHeadIndices.Sort();

        // Determine which wrong head this is (1st or 2nd) based on placement index order
        int wrongHeadNumber = wrongHeadIndices.IndexOf(placement.placementIndex) + 1;

        Debug.Log($"[HeadShrinePuzzle] This is wrong head #{wrongHeadNumber} (out of {wrongHeadIndices.Count} total wrong heads)");
        Debug.Log($"[HeadShrinePuzzle] Wrong head placement indices: [{string.Join(", ", wrongHeadIndices)}]");

        // Open the appropriate door based on which wrong head this is - using Force methods for guaranteed opening
        if (wrongHeadNumber == 1) // First wrong head
        {
            Debug.Log($"[HeadShrinePuzzle] 🚪 Processing FIRST wrong head");
            if (wrongHead1Door && wrongHead1Door.gameObject != null)
            {
                Debug.Log($"[HeadShrinePuzzle] Force unlocking and opening wrong head 1 door: {wrongHead1Door.gameObject.name}");
                Debug.Log($"[HeadShrinePuzzle] Door state before: isLocked={wrongHead1Door.IsLocked()}, isOpen={wrongHead1Door.IsOpen()}");

                wrongHead1Door.ForceUnlock();  // Immediate unlock without coroutines
                wrongHead1Door.ForceOpen();    // Immediate open without animation

                if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);

                Debug.Log($"[HeadShrinePuzzle] Door state after: isLocked={wrongHead1Door.IsLocked()}, isOpen={wrongHead1Door.IsOpen()}");
                Debug.Log("[HeadShrinePuzzle] ✅ Wrong head 1 door force unlocked and opened!");
            }
            else
            {
                Debug.LogWarning("[HeadShrinePuzzle] ❌ Wrong head 1 door not assigned or GameObject is null!");
            }
        }
        else if (wrongHeadNumber == 2) // Second wrong head
        {
            Debug.Log($"[HeadShrinePuzzle] 🚪 Processing SECOND wrong head");
            if (wrongHead2Door && wrongHead2Door.gameObject != null)
            {
                Debug.Log($"[HeadShrinePuzzle] Force unlocking and opening wrong head 2 door: {wrongHead2Door.gameObject.name}");
                Debug.Log($"[HeadShrinePuzzle] Door state before: isLocked={wrongHead2Door.IsLocked()}, isOpen={wrongHead2Door.IsOpen()}");

                wrongHead2Door.ForceUnlock();  // Immediate unlock without coroutines
                wrongHead2Door.ForceOpen();    // Immediate open without animation

                if (doorOpenSound) audioSource.PlayOneShot(doorOpenSound);

                Debug.Log($"[HeadShrinePuzzle] Door state after: isLocked={wrongHead2Door.IsLocked()}, isOpen={wrongHead2Door.IsOpen()}");
                Debug.Log("[HeadShrinePuzzle] ✅ Wrong head 2 door force unlocked and opened!");
            }
            else
            {
                Debug.LogWarning("[HeadShrinePuzzle] ❌ Wrong head 2 door not assigned or GameObject is null!");
            }
        }
        else
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ❌ Unexpected wrong head number: {wrongHeadNumber}. Only 2 wrong heads are supported (wrongHead1Door and wrongHead2Door).");
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
        Debug.Log($"[HeadShrinePuzzle] Cleaning up interactive components from head object: {headObj.name}");

        // Remove all interactive components but preserve visual components
        Rigidbody[] rbs = headObj.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (rb)
            {
                Debug.Log($"[HeadShrinePuzzle] Removing Rigidbody from {rb.gameObject.name}");
                Destroy(rb);
            }
        }

        Collider[] cols = headObj.GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col)
            {
                Debug.Log($"[HeadShrinePuzzle] Removing Collider from {col.gameObject.name}");
                Destroy(col);
            }
        }

        DullahanHeadPickable[] pickables = headObj.GetComponentsInChildren<DullahanHeadPickable>(true);
        foreach (var pickable in pickables)
        {
            if (pickable)
            {
                Debug.Log($"[HeadShrinePuzzle] Removing DullahanHeadPickable from {pickable.gameObject.name}");
                Destroy(pickable);
            }
        }

        // Verify the head object still has renderer components
        Renderer[] renderers = headObj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Debug.Log($"[HeadShrinePuzzle] ✅ Head object has {renderers.Length} renderer(s) preserved");
            foreach (var renderer in renderers)
            {
                // Ensure renderers are enabled and visible
                renderer.enabled = true;
                Debug.Log($"[HeadShrinePuzzle] Renderer on {renderer.gameObject.name}: enabled={renderer.enabled}");
            }
        }
        else
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ No renderers found on head object {headObj.name}!");
        }
    }
    
    void CreateHeadObjectOnShrine(DullahanHeadSO head, ShrinePlacement placement)
    {
        Debug.Log($"[HeadShrinePuzzle] 🎭 Creating head object on shrine: {head.headName}");

        // Create head object from prefab (like pickup but in reverse)
        if (head.headPrefab)
        {
            GameObject headObject = Instantiate(head.headPrefab, placement.placementTransform);
            headObject.name = $"{head.headName}_Placed"; // Rename for clarity
            headObject.transform.localPosition = new Vector3(0, 0.3f, 0);
            headObject.transform.localRotation = Quaternion.identity;
            headObject.transform.localScale = Vector3.one * 50f;

            // Ensure the head object is active
            headObject.SetActive(true);

            // Store the created head object
            placement.spawnedHeadModel = headObject;

            Debug.Log($"[HeadShrinePuzzle] Head object instantiated: {headObject.name} at position {headObject.transform.position}");
            Debug.Log($"[HeadShrinePuzzle] Local position: {headObject.transform.localPosition}, Scale: {headObject.transform.localScale}");

            // Setup head object like pickup system but in reverse
            SetupHeadObjectOnShrine(headObject, head);

            // Verify visibility after setup
            Renderer[] renderers = headObject.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[HeadShrinePuzzle] Final check - Head has {renderers.Length} renderer(s)");
            foreach (var renderer in renderers)
            {
                Debug.Log($"[HeadShrinePuzzle] - Renderer on {renderer.gameObject.name}: active={renderer.gameObject.activeInHierarchy}, enabled={renderer.enabled}");
            }

            Debug.Log($"[HeadShrinePuzzle] ✅ Head object created successfully on shrine!");
        }
        else
        {
            Debug.LogWarning($"[HeadShrinePuzzle] ⚠️ No head prefab assigned for {head.headName}");
        }
    }
    
    void SetupHeadObjectOnShrine(GameObject headObject, DullahanHeadSO headData)
    {
        Debug.Log($"[HeadShrinePuzzle] 🎨 Setting up head object on shrine...");

        // Setup visual effects FIRST (before cleanup)
        SetupHeadVisualEffects(headObject, headData);

        // Remove interactive components so it can't be picked up again
        // This is done AFTER visual setup to ensure visuals are preserved
        CleanupHeadComponents(headObject);

        // Note: We're NOT adding DullahanHeadPickable component because we don't want
        // the head to be pickable once placed on the shrine
        Debug.Log($"[HeadShrinePuzzle] ✅ Head object setup complete!");
    }
    
    void SetupHeadVisualEffects(GameObject headObject, DullahanHeadSO headData)
    {
        Debug.Log($"[HeadShrinePuzzle] Setting up visual effects for {headData.headName}");

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
            headLight.enabled = true;
            Debug.Log($"[HeadShrinePuzzle] Added glow effect: color={headData.headGlowColor}, intensity=1, range=3");
        }

        // Setup material (like pickup system)
        // Check both the main object and all children for renderers
        Renderer[] allRenderers = headObject.GetComponentsInChildren<Renderer>(true);
        if (allRenderers.Length > 0 && headData.headMaterial != null)
        {
            Debug.Log($"[HeadShrinePuzzle] Found {allRenderers.Length} renderer(s), applying head material");
            foreach (var renderer in allRenderers)
            {
                renderer.material = headData.headMaterial;
                renderer.enabled = true; // Ensure enabled
                Debug.Log($"[HeadShrinePuzzle] Applied material to renderer on {renderer.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"[HeadShrinePuzzle] No material applied (renderers: {allRenderers.Length}, material: {headData.headMaterial != null})");
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
            Debug.Log($"[HeadShrinePuzzle] Playing placement sound");
        }

        Debug.Log($"[HeadShrinePuzzle] Visual effects setup complete");
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

    // Helper methods for sequential placement logic

    /// <summary>
    /// Check if all placements have heads
    /// </summary>
    bool AreAllPlacementsFull()
    {
        foreach (ShrinePlacement placement in placements)
        {
            if (!placement.hasHead)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get the next empty placement in sequence (Left → Right → Center)
    /// </summary>
    ShrinePlacement GetNextEmptyPlacement()
    {
        for (int i = 0; i < placements.Count; i++)
        {
            if (!placements[i].hasHead)
                return placements[i];
        }
        return null; // All placements are full
    }

    /// <summary>
    /// Get direction name for UI prompts (Left, Right, Center)
    /// </summary>
    string GetPlacementDirectionName(int placementIndex)
    {
        if (placementIndex == 0) return "Left";
        if (placementIndex == 1) return "Right";
        if (placementIndex == 2) return "Center";
        return $"Placement {placementIndex + 1}";
    }

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

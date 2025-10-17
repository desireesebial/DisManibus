using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 🏛️ HEAD SHRINE PUZZLE - Perfect for Indie Developers!
/// 
/// A mystical shrine with a single altar that has multiple placement points.
/// Each placement point (empty GameObject) can hold a specific Dullahan head.
/// The real head must be placed in the center position between the candles.
/// When all required heads are placed, the shrine activates and grants rewards.
/// 
/// SETUP (3 steps):
/// 1. Create empty GameObject with this script
/// 2. Create child objects for placement points (name them "Placement1", "Placement2", etc.)
/// 3. Assign head IDs and materials in inspector
/// 
/// THAT'S IT! No complex setup needed.
/// </summary>
public class HeadShrinePuzzle : MonoBehaviour
{
    [Header("🏛️ Shrine Settings")]
    [Tooltip("List of head IDs that need to be placed on placement points (in order)")]
    public int[] requiredHeadIDs = { 1, 2, 3 }; // Example: Real head, Fake head 1, Fake head 2
    
    [Tooltip("Which placement point requires the REAL head (0 = first, 1 = second, etc.)")]
    public int realHeadPlacementIndex = 0; // Center placement point
    
    [Tooltip("How close player needs to be to interact with any placement point")]
    public float interactionDistance = 4f;
    
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
    public AudioClip mysticalChantingSound;
    
    [Header("🎁 Rewards")]
    public Door rewardDoor;
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
        public GameObject placedHead;
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
        
        // Find all child objects that could be placement points
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Check if this looks like a placement point
            if (child.name.ToLower().Contains("placement") || 
                child.name.StartsWith("Placement") ||
                (i < requiredHeadIDs.Length))
            {
                int headID = (i < requiredHeadIDs.Length) ? requiredHeadIDs[i] : 0;
                bool isRealHead = (i == realHeadPlacementIndex);
                ShrinePlacement placement = new ShrinePlacement(child, headID, isRealHead);
                placements.Add(placement);
                
                // Setup placement appearance
                SetupPlacementAppearance(placement);
                
                Debug.Log($"[HeadShrinePuzzle] Created placement: {child.name} for head ID {headID} (Real head: {isRealHead})");
            }
        }
        
        // If no placements found, create them automatically
        if (placements.Count == 0)
        {
            CreatePlacementsAutomatically();
        }
    }
    
    void CreatePlacementsAutomatically()
    {
        Debug.Log("[HeadShrinePuzzle] No placements found, creating automatically...");
        
        // Create altar base
        GameObject altarBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altarBase.name = "AltarBase";
        altarBase.transform.SetParent(transform);
        altarBase.transform.localPosition = Vector3.zero;
        altarBase.transform.localScale = new Vector3(4f, 0.2f, 2f);
        
        // Apply altar base material
        if (altarBaseMaterial && altarBase.GetComponent<Renderer>())
        {
            altarBase.GetComponent<Renderer>().material = altarBaseMaterial;
        }
        
        // Remove collider
        Destroy(altarBase.GetComponent<Collider>());
        
        // Create placement points
        for (int i = 0; i < requiredHeadIDs.Length; i++)
        {
            // Create placement point
            GameObject placementObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placementObj.name = $"Placement{i + 1}";
            placementObj.transform.SetParent(transform);
            
            // Position placements on the altar (center, left, right)
            if (i == 0) // Center (real head)
            {
                placementObj.transform.localPosition = new Vector3(0, 0.2f, 0);
            }
            else if (i == 1) // Left
            {
                placementObj.transform.localPosition = new Vector3(-1.5f, 0.2f, 0);
            }
            else // Right
            {
                placementObj.transform.localPosition = new Vector3(1.5f, 0.2f, 0);
            }
            
            placementObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            
            // Remove collider
            Destroy(placementObj.GetComponent<Collider>());
            
            bool isRealHead = (i == realHeadPlacementIndex);
            ShrinePlacement placement = new ShrinePlacement(placementObj.transform, requiredHeadIDs[i], isRealHead);
            placements.Add(placement);
            
            // Setup appearance
            SetupPlacementAppearance(placement);
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
        foreach (ShrinePlacement placement in placements)
        {
            float distance = Vector3.Distance(placement.placementTransform.position, player.position);
            if (distance <= interactionDistance)
            {
                // Check if player has a head and presses F
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TryPlaceHeadOnPlacement(placement);
                }
                break; // Only check one placement at a time
            }
        }
    }
    
    void TryPlaceHeadOnPlacement(ShrinePlacement placement)
    {
        // Get current head from inventory
        DullahanHeadSO currentHead = inventory.GetCurrentHead();
        if (currentHead == null)
        {
            Debug.Log("[HeadShrinePuzzle] No head in inventory");
            return;
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
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Mark placement as having a head
        placement.hasHead = true;
        
        // Create visual head on placement
        if (head.headPrefab)
        {
            placement.placedHead = Instantiate(head.headPrefab, placement.placementTransform);
            placement.placedHead.transform.localPosition = new Vector3(0, 0.3f, 0);
            placement.placedHead.transform.localRotation = Quaternion.identity;
            placement.placedHead.transform.localScale = Vector3.one * 0.8f;
            
            // Remove interactive components
            CleanupHeadComponents(placement.placedHead);
        }
        
        // Update placement material
        if (placement.placementRenderer && filledPlacementMaterial)
        {
            placement.placementRenderer.material = filledPlacementMaterial;
        }
        
        // Activate the placement
        ActivatePlacement(placement);
        
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
        
        // Grant rewards
        if (rewardDoor) rewardDoor.UnlockDoor();
        
        if (rewardItems != null && rewardSpawnPoint != null)
        {
            foreach (GameObject item in rewardItems)
            {
                if (item) Instantiate(item, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
            }
        }
        
        // Notify event managers
        Floor2EndingEventManager eventManager = FindObjectOfType<Floor2EndingEventManager>();
        if (eventManager) eventManager.OnRealHeadAttached();
        
        Debug.Log("[HeadShrinePuzzle] All rewards granted!");
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
        
        // Draw placement positions
        Gizmos.color = Color.yellow;
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

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SIMPLE Head Collection Puzzle - Perfect for Beginner Developers!
/// 
/// Instead of placing heads on moving Dullahan, player brings heads to a static pedestal.
/// Each head has its own slot. When all correct heads are placed, puzzle completes.
/// 
/// SETUP (3 steps):
/// 1. Create empty GameObject with this script
/// 2. Create child objects for each head slot (name them "Slot1", "Slot2", etc.)
/// 3. Assign head IDs in inspector
/// 
/// THAT'S IT! No complex setup needed.
/// </summary>
public class SimpleHeadCollectionPuzzle : MonoBehaviour
{
    [Header("🎯 Puzzle Settings")]
    [Tooltip("List of head IDs that need to be collected (in order)")]
    public int[] requiredHeadIDs = { 1, 2, 3 }; // Example: Real head, Fake head 1, Fake head 2
    
    [Tooltip("How close player needs to be to interact")]
    public float interactionDistance = 3f;
    
    [Header("🎨 Visual Settings")]
    [Tooltip("Material for empty slots")]
    public Material emptySlotMaterial;
    
    [Tooltip("Material for filled slots")]
    public Material filledSlotMaterial;
    
    [Tooltip("Material for correct head in slot")]
    public Material correctHeadMaterial;
    
    [Header("🎵 Audio (Optional)")]
    public AudioClip headPlacedSound;
    public AudioClip puzzleCompleteSound;
    public AudioClip wrongHeadSound;
    
    [Header("🎁 Rewards (Optional)")]
    public Door rewardDoor;
    public GameObject[] rewardItems;
    
    // Private variables
    private Transform player;
    private DullahanHeadInventory inventory;
    private AudioSource audioSource;
    private List<HeadSlot> headSlots = new List<HeadSlot>();
    private bool puzzleComplete = false;
    
    // Simple slot class
    [System.Serializable]
    public class HeadSlot
    {
        public Transform slotTransform;
        public Renderer slotRenderer;
        public int requiredHeadID;
        public bool isFilled = false;
        public GameObject placedHead;
        
        public HeadSlot(Transform transform, int headID)
        {
            slotTransform = transform;
            requiredHeadID = headID;
            slotRenderer = transform.GetComponent<Renderer>();
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
        
        // Setup head slots automatically
        SetupHeadSlots();
        
        Debug.Log($"[SimpleHeadCollectionPuzzle] Setup complete! {headSlots.Count} slots created.");
    }
    
    void SetupHeadSlots()
    {
        // Clear existing slots
        headSlots.Clear();
        
        // Find all child objects that could be slots
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // Check if this looks like a slot (has "Slot" in name or is a numbered child)
            if (child.name.ToLower().Contains("slot") || 
                child.name.StartsWith("Slot") ||
                (i < requiredHeadIDs.Length))
            {
                int headID = (i < requiredHeadIDs.Length) ? requiredHeadIDs[i] : 0;
                HeadSlot slot = new HeadSlot(child, headID);
                headSlots.Add(slot);
                
                // Set initial material
                if (emptySlotMaterial && slot.slotRenderer)
                {
                    slot.slotRenderer.material = emptySlotMaterial;
                }
                
                Debug.Log($"[SimpleHeadCollectionPuzzle] Created slot: {child.name} for head ID {headID}");
            }
        }
        
        // If no slots found, create them automatically
        if (headSlots.Count == 0)
        {
            CreateSlotsAutomatically();
        }
    }
    
    void CreateSlotsAutomatically()
    {
        Debug.Log("[SimpleHeadCollectionPuzzle] No slots found, creating automatically...");
        
        for (int i = 0; i < requiredHeadIDs.Length; i++)
        {
            // Create a simple cube for each slot
            GameObject slotObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slotObj.name = $"Slot{i + 1}";
            slotObj.transform.SetParent(transform);
            slotObj.transform.localPosition = new Vector3(i * 2f, 0, 0); // Space them out
            slotObj.transform.localScale = Vector3.one * 0.5f;
            
            // Remove collider (we don't need physics)
            Collider col = slotObj.GetComponent<Collider>();
            if (col) Destroy(col);
            
            HeadSlot slot = new HeadSlot(slotObj.transform, requiredHeadIDs[i]);
            headSlots.Add(slot);
            
            // Set material
            if (emptySlotMaterial && slot.slotRenderer)
            {
                slot.slotRenderer.material = emptySlotMaterial;
            }
        }
    }
    
    void Update()
    {
        if (puzzleComplete || !player || !inventory) return;
        
        // Check if player is close enough
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionDistance)
        {
            // Check if player has a head and presses F
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryPlaceHead();
            }
        }
    }
    
    void TryPlaceHead()
    {
        // Get current head from inventory
        DullahanHeadSO currentHead = inventory.GetCurrentHead();
        if (currentHead == null)
        {
            Debug.Log("[SimpleHeadCollectionPuzzle] No head in inventory");
            return;
        }
        
        Debug.Log($"[SimpleHeadCollectionPuzzle] Trying to place head: {currentHead.headName} (ID: {currentHead.headID})");
        
        // Find the correct slot for this head
        HeadSlot targetSlot = null;
        foreach (HeadSlot slot in headSlots)
        {
            if (slot.requiredHeadID == currentHead.headID && !slot.isFilled)
            {
                targetSlot = slot;
                break;
            }
        }
        
        if (targetSlot != null)
        {
            PlaceHeadInSlot(currentHead, targetSlot);
        }
        else
        {
            // Wrong head or slot already filled
            HandleWrongHead(currentHead);
        }
    }
    
    void PlaceHeadInSlot(DullahanHeadSO head, HeadSlot slot)
    {
        Debug.Log($"[SimpleHeadCollectionPuzzle] ✓ Placing {head.headName} in slot!");
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Mark slot as filled
        slot.isFilled = true;
        
        // Create visual head in slot
        if (head.headPrefab)
        {
            slot.placedHead = Instantiate(head.headPrefab, slot.slotTransform);
            slot.placedHead.transform.localPosition = Vector3.zero;
            slot.placedHead.transform.localRotation = Quaternion.identity;
            slot.placedHead.transform.localScale = Vector3.one;
            
            // Remove interactive components
            CleanupHeadComponents(slot.placedHead);
        }
        
        // Update slot material
        if (slot.slotRenderer)
        {
            slot.slotRenderer.material = correctHeadMaterial ? correctHeadMaterial : filledSlotMaterial;
        }
        
        // Play sound
        if (headPlacedSound) audioSource.PlayOneShot(headPlacedSound);
        
        // Check if puzzle is complete
        CheckPuzzleCompletion();
    }
    
    void HandleWrongHead(DullahanHeadSO head)
    {
        Debug.Log($"[SimpleHeadCollectionPuzzle] ✗ Wrong head: {head.headName}");
        
        // Remove from inventory
        inventory.RemoveSelectedHeadIfHead();
        
        // Play wrong sound
        if (wrongHeadSound) audioSource.PlayOneShot(wrongHeadSound);
        
        // Could add visual feedback here (screen flash, etc.)
    }
    
    void CheckPuzzleCompletion()
    {
        // Check if all slots are filled
        bool allFilled = true;
        foreach (HeadSlot slot in headSlots)
        {
            if (!slot.isFilled)
            {
                allFilled = false;
                break;
            }
        }
        
        if (allFilled)
        {
            CompletePuzzle();
        }
    }
    
    void CompletePuzzle()
    {
        Debug.Log("[SimpleHeadCollectionPuzzle] 🎉 PUZZLE COMPLETED!");
        
        puzzleComplete = true;
        
        // Play completion sound
        if (puzzleCompleteSound) audioSource.PlayOneShot(puzzleCompleteSound);
        
        // Grant rewards
        if (rewardDoor) rewardDoor.UnlockDoor();
        
        if (rewardItems != null)
        {
            foreach (GameObject item in rewardItems)
            {
                if (item) Instantiate(item, transform.position + Vector3.up, Quaternion.identity);
            }
        }
        
        // NOTE: Floor2EndingEventManager is no longer needed - functionality moved to DullahanChaseEventManager
        // Event manager notification removed as it's no longer required for the simplified chase system
        // The chase cycle now operates independently based on proximity detection

        Debug.Log("[SimpleHeadCollectionPuzzle] All rewards granted!");
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
    public bool IsPuzzleComplete() => puzzleComplete;
    
    public int GetFilledSlotsCount()
    {
        int count = 0;
        foreach (HeadSlot slot in headSlots)
        {
            if (slot.isFilled) count++;
        }
        return count;
    }
    
    public int GetTotalSlotsCount() => headSlots.Count;
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Draw slot positions
        Gizmos.color = Color.yellow;
        for (int i = 0; i < headSlots.Count; i++)
        {
            if (headSlots[i].slotTransform)
            {
                Gizmos.DrawWireCube(headSlots[i].slotTransform.position, Vector3.one * 0.5f);
            }
        }
    }
}

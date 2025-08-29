using UnityEngine;
using System.Collections.Generic;

public class DullahanPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public DullahanHeadPickable[] headPickables = new DullahanHeadPickable[3];
    public DullahanBody dullahanBody;
    public DullahanChaseSystem dullahanChase;
    public doorscript[] puzzleDoors; // Doors that open when puzzle is completed
    public doorscript realHeadDoor; // Door to the real head room
    
    [Header("Managers")]
    public DullahanHeadInventory headInventory;
    public DullahanHeadEffectManager effectManager;
    public DullahanAudioManager audioManager;
    
    [Header("Puzzle Settings")]
    public bool puzzleActive = true;
    public bool debugMode = false;
    
    [Header("UI")]
    public GameObject puzzleUI;
    public TMPro.TextMeshProUGUI puzzleStatusText;
    
    // Puzzle state
    private bool puzzleCompleted = false;
    private List<DullahanHeadSO> collectedHeads = new List<DullahanHeadSO>();
    private bool isInitialized = false;
    
    void Start()
    {
        // Find missing references
        FindMissingReferences();
        
        // Setup puzzle
        SetupPuzzle();
        
        // Setup UI
        SetupUI();
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized || !puzzleActive) return;
        
        // Update puzzle status
        UpdatePuzzleStatus();
        
        // Check for puzzle completion
        CheckPuzzleCompletion();
        
        // Handle debug input
        if (debugMode)
        {
            HandleDebugInput();
        }
    }
    
    private void FindMissingReferences()
    {
        // Find head pickables if not assigned
        if (headPickables.Length == 0 || headPickables[0] == null)
        {
            DullahanHeadPickable[] foundPickables = FindObjectsOfType<DullahanHeadPickable>();
            if (foundPickables.Length > 0)
            {
                headPickables = foundPickables;
            }
        }
        
        // Find Dullahan body if not assigned
        if (dullahanBody == null)
            dullahanBody = FindObjectOfType<DullahanBody>();
        
        // Find Dullahan chase if not assigned
        if (dullahanChase == null)
            dullahanChase = FindObjectOfType<DullahanChaseSystem>();
        
        // Find head inventory if not assigned
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
        
        // Find effect manager if not assigned
        if (effectManager == null)
            effectManager = FindObjectOfType<DullahanHeadEffectManager>();
        
        // Find audio manager if not assigned
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
            
        // Find real head door if not assigned
        if (realHeadDoor == null)
        {
            doorscript[] allDoors = FindObjectsOfType<doorscript>();
            foreach (var door in allDoors)
            {
                if (door.name.ToLower().Contains("real") || door.name.ToLower().Contains("head"))
                {
                    realHeadDoor = door;
                    break;
                }
            }
        }
    }
    
    private void SetupPuzzle()
    {
        // Validate head pickables
        ValidateHeadPickables();
        
        // Setup head inventory if not already done
        if (headInventory != null)
        {
            headInventory.ClearInventoryList();
            headInventory.selectedItem = -1;
        }
        
        // Reset puzzle state
        puzzleCompleted = false;
        collectedHeads.Clear();
        
        // Enable all head pickables
        foreach (DullahanHeadPickable pickable in headPickables)
        {
            if (pickable != null)
            {
                pickable.isPickedUp = false;
                pickable.gameObject.SetActive(true);
            }
        }
        
        // Reset Dullahan body
        if (dullahanBody != null)
        {
            dullahanBody.ResetPuzzle();
        }
        
        // Lock all doors initially
        LockAllDoors();
        
        Debug.Log("Dullahan puzzle initialized");
    }
    
    private void ValidateHeadPickables()
    {
        List<DullahanHeadPickable> validPickables = new List<DullahanHeadPickable>();
        
        foreach (DullahanHeadPickable pickable in headPickables)
        {
            if (pickable != null && pickable.headData != null)
            {
                validPickables.Add(pickable);
            }
            else if (pickable != null && pickable.headData == null)
            {
                Debug.LogWarning($"Head pickable {pickable.name} has no head data assigned!");
            }
        }
        
        headPickables = validPickables.ToArray();
        
        if (headPickables.Length < 3)
        {
            Debug.LogWarning($"Only {headPickables.Length} valid head pickables found. Expected 3.");
        }
    }
    
    private void SetupUI()
    {
        if (puzzleUI != null)
            puzzleUI.SetActive(true);
        
        UpdatePuzzleUI();
    }
    
    private void UpdatePuzzleStatus()
    {
        // Update collected heads list
        if (headInventory != null)
        {
            collectedHeads.Clear();
            collectedHeads.AddRange(headInventory.inventoryList);
        }
        
        // Check if puzzle is completed
        if (dullahanBody != null)
        {
            puzzleCompleted = dullahanBody.IsPuzzleCompleted();
        }
    }
    
    private void CheckPuzzleCompletion()
    {
        if (puzzleCompleted && puzzleActive)
        {
            OnPuzzleCompleted();
        }
    }
    
    private void OnPuzzleCompleted()
    {
        puzzleActive = false;
        
        // Play completion sound
        if (audioManager != null)
        {
            audioManager.PlayPuzzleCompleteSound();
        }
        
        // Stop Dullahan chase
        if (dullahanChase != null)
        {
            // This would need to be implemented in DullahanChaseSystem
            // dullahanChase.StopChase();
        }
        
        // Clear all effects
        if (effectManager != null)
        {
            effectManager.ClearAllEffects();
        }
        
        // Open puzzle doors
        OpenPuzzleDoors();
        
        // Update UI
        UpdatePuzzleUI();
        
        Debug.Log("Dullahan puzzle completed!");
    }
    
    private void UpdatePuzzleUI()
    {
        if (puzzleStatusText == null) return;
        
        if (puzzleCompleted)
        {
            puzzleStatusText.text = "Puzzle Completed! The door is now open.";
        }
        else
        {
            int headsCollected = collectedHeads.Count;
            int totalHeads = headPickables.Length;
            puzzleStatusText.text = $"Heads Collected: {headsCollected}/{totalHeads}\nFind the real head and attach it to the Dullahan body.";
        }
    }
    
    private void HandleDebugInput()
    {
        // Debug: Complete puzzle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("Debug: Completing puzzle");
            if (dullahanBody != null)
            {
                // Create a fake real head for testing
                DullahanHeadSO fakeRealHead = ScriptableObject.CreateInstance<DullahanHeadSO>();
                fakeRealHead.headID = dullahanBody.requiredHeadID;
                fakeRealHead.headName = "Debug Real Head";
                fakeRealHead.headType = HeadType.Real;
                
                dullahanBody.AttachHead(fakeRealHead);
            }
        }
        
        // Debug: Reset puzzle
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("Debug: Resetting puzzle");
            ResetPuzzle();
        }
        
        // Debug: Add all heads to inventory
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("Debug: Adding all heads to inventory");
            if (headInventory != null)
            {
                foreach (DullahanHeadPickable pickable in headPickables)
                {
                    if (pickable != null && pickable.headData != null && !pickable.isPickedUp)
                    {
                        headInventory.AddToInventoryList(pickable.headData);
                        pickable.isPickedUp = true;
                        pickable.gameObject.SetActive(false);
                    }
                }
                
                if (headInventory.inventoryList.Count > 0)
                {
                    headInventory.selectedItem = 0;
                    headInventory.NewItemSelected();
                }
            }
        }
    }
    
    // Public methods for other scripts
    public void ResetPuzzle()
    {
        puzzleActive = true;
        puzzleCompleted = false;
        collectedHeads.Clear();
        
        // Reset head inventory
        if (headInventory != null)
        {
            headInventory.ClearInventoryList();
            headInventory.selectedItem = -1;
            headInventory.DeactivateAllItems();
        }
        
        // Reset head pickables
        foreach (DullahanHeadPickable pickable in headPickables)
        {
            if (pickable != null)
            {
                pickable.isPickedUp = false;
                pickable.gameObject.SetActive(true);
            }
        }
        
        // Reset Dullahan body
        if (dullahanBody != null)
        {
            dullahanBody.ResetPuzzle();
        }
        
        // Clear all effects
        if (effectManager != null)
        {
            effectManager.ClearAllEffects();
        }
        
        // Update UI
        UpdatePuzzleUI();
        
        Debug.Log("Dullahan puzzle reset");
    }
    
    private void OpenPuzzleDoors()
    {
        if (puzzleDoors != null && puzzleDoors.Length > 0)
        {
            foreach (var door in puzzleDoors)
            {
                if (door != null)
                {
                    door.ForceUnlock();
                    door.OpenDoor();
                    Debug.Log($"Puzzle door {door.name} opened!");
                }
            }
        }
        
        // Also open real head door if puzzle is completed
        if (realHeadDoor != null)
        {
            realHeadDoor.ForceUnlock();
            realHeadDoor.OpenDoor();
            Debug.Log($"Real head door {realHeadDoor.name} opened!");
        }
    }
    
    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }
    
    public int GetCollectedHeadsCount()
    {
        return collectedHeads.Count;
    }
    
    public int GetTotalHeadsCount()
    {
        return headPickables.Length;
    }
    
    public bool HasRealHead()
    {
        foreach (DullahanHeadSO head in collectedHeads)
        {
            if (head != null && head.headType == HeadType.Real)
                return true;
        }
        return false;
    }
    
    public DullahanHeadSO GetRealHead()
    {
        foreach (DullahanHeadSO head in collectedHeads)
        {
            if (head != null && head.headType == HeadType.Real)
                return head;
        }
        return null;
    }
    
    // Event handlers for head pickup
    public void OnHeadPickedUp(DullahanHeadSO headData)
    {
        if (headData != null)
        {
            collectedHeads.Add(headData);
            UpdatePuzzleUI();
            
            Debug.Log($"Head picked up: {headData.headName}");
        }
    }
    
    public void OnHeadAttached(DullahanHeadSO headData)
    {
        if (headData != null)
        {
            collectedHeads.Remove(headData);
            UpdatePuzzleUI();
            
            Debug.Log($"Head attached: {headData.headName}");
        }
    }
    
    // Method called by the new event manager to spawn all heads
    public void SpawnAllHeads()
    {
        Debug.Log("Spawning all three Dullahan heads for collection phase");
        
        // Reset and enable all head pickables
        foreach (DullahanHeadPickable pickable in headPickables)
        {
            if (pickable != null)
            {
                pickable.isPickedUp = false;
                pickable.gameObject.SetActive(true);
                
                // Reset any visual states
                if (pickable.headVisual != null)
                {
                    pickable.headVisual.SetActive(true);
                }
            }
        }
        
        // Clear head inventory
        if (headInventory != null)
        {
            headInventory.ClearInventoryList();
            headInventory.selectedItem = -1;
            headInventory.DeactivateAllItems();
        }
        
        // Clear collected heads list
        collectedHeads.Clear();
        
        // Open real head door for collection phase
        if (realHeadDoor != null)
        {
            realHeadDoor.ForceUnlock();
            realHeadDoor.OpenDoor();
            Debug.Log("Real head door opened for collection phase");
        }
        
        // Update UI
        UpdatePuzzleUI();
        
        Debug.Log("All three heads spawned and ready for collection");
    }
    
    // Door management methods
    private void LockAllDoors()
    {
        // Lock puzzle doors
        if (puzzleDoors != null && puzzleDoors.Length > 0)
        {
            foreach (var door in puzzleDoors)
            {
                if (door != null)
                {
                    door.LockDoor();
                    door.CloseDoor();
                }
            }
        }
        
        // Lock real head door
        if (realHeadDoor != null)
        {
            realHeadDoor.LockDoor();
            realHeadDoor.CloseDoor();
        }
        
        Debug.Log("All puzzle doors locked");
    }
    
    public void OpenRealHeadDoor()
    {
        if (realHeadDoor != null)
        {
            realHeadDoor.ForceUnlock();
            realHeadDoor.OpenDoor();
            Debug.Log("Real head door opened");
        }
    }
    
    public void CloseRealHeadDoor()
    {
        if (realHeadDoor != null)
        {
            realHeadDoor.CloseDoor();
            Debug.Log("Real head door closed");
        }
    }
}

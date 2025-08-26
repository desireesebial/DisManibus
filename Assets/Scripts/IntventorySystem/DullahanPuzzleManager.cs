using UnityEngine;
using System.Collections.Generic;

public class DullahanPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Components")]
    public DullahanHeadPickable[] headPickables = new DullahanHeadPickable[3];
    public DullahanBody dullahanBody;
    public DullahanChaseSystem dullahanChase;
    
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
    }
    
    private void SetupPuzzle()
    {
        // Validate head pickables
        ValidateHeadPickables();
        
        // Setup head inventory if not already done
        if (headInventory != null)
        {
            headInventory.headInventoryList.Clear();
            headInventory.selectedHeadIndex = -1;
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
            collectedHeads.AddRange(headInventory.headInventoryList);
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
                        headInventory.headInventoryList.Add(pickable.headData);
                        pickable.isPickedUp = true;
                        pickable.gameObject.SetActive(false);
                    }
                }
                
                if (headInventory.headInventoryList.Count > 0)
                {
                    headInventory.selectedHeadIndex = 0;
                    headInventory.NewHeadSelected();
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
            headInventory.headInventoryList.Clear();
            headInventory.selectedHeadIndex = -1;
            headInventory.DeactivateAllHeads();
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
}

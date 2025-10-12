using UnityEngine;

/// <summary>
/// Example script showing how to integrate the Dullahan Head Placement Puzzle
/// with other game systems. This can be used as a template for custom integrations.
/// </summary>
public class ExamplePuzzleIntegration : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the head placement puzzle")]
    public DullahanHeadPlacementPuzzle headPlacementPuzzle;
    
    [Tooltip("Reference to the head placeholder")]
    public HeadPlaceholder headPlaceholder;
    
    [Header("Custom Events")]
    [Tooltip("Enable custom event handling")]
    public bool enableCustomEvents = true;
    
    [Header("Custom Rewards")]
    [Tooltip("GameObject to activate when puzzle completes")]
    public GameObject customRewardObject;
    
    [Tooltip("NPC to enable when puzzle completes")]
    public GameObject npcToEnable;
    
    [Tooltip("Teleport player to this location when puzzle completes")]
    public Transform teleportDestination;
    
    private bool puzzleWasCompleted = false;
    
    void Start()
    {
        // Find puzzle if not assigned
        if (headPlacementPuzzle == null)
        {
            headPlacementPuzzle = FindObjectOfType<DullahanHeadPlacementPuzzle>();
        }
        
        // Find placeholder if not assigned
        if (headPlaceholder == null)
        {
            headPlaceholder = FindObjectOfType<HeadPlaceholder>();
        }
    }
    
    void Update()
    {
        // Check for puzzle completion
        if (enableCustomEvents && headPlacementPuzzle != null)
        {
            CheckPuzzleCompletion();
        }
    }
    
    private void CheckPuzzleCompletion()
    {
        // Check if puzzle was just completed this frame
        if (!puzzleWasCompleted && headPlacementPuzzle.IsPuzzleCompleted())
        {
            OnPuzzleCompleted();
            puzzleWasCompleted = true;
        }
    }
    
    /// <summary>
    /// Called when puzzle is completed
    /// </summary>
    private void OnPuzzleCompleted()
    {
        Debug.Log("[ExampleIntegration] Puzzle completed! Handling custom events...");
        
        // Activate custom reward object
        if (customRewardObject != null)
        {
            customRewardObject.SetActive(true);
            Debug.Log("[ExampleIntegration] Activated custom reward object");
        }
        
        // Enable NPC
        if (npcToEnable != null)
        {
            npcToEnable.SetActive(true);
            Debug.Log("[ExampleIntegration] Enabled NPC");
        }
        
        // Teleport player
        if (teleportDestination != null)
        {
            TeleportPlayer();
        }
        
        // Add your custom logic here!
        CustomPuzzleCompletionLogic();
    }
    
    /// <summary>
    /// Add your custom puzzle completion logic here
    /// </summary>
    private void CustomPuzzleCompletionLogic()
    {
        // Example: Save game progress
        // SaveSystem.SetPuzzleCompleted("DullahanHeadPlacement", true);
        
        // Example: Update quest
        // QuestManager.CompleteObjective("PlaceHeadOnDullahan");
        
        // Example: Trigger cutscene
        // CutsceneManager.PlayCutscene("DullahanHeadPlaced");
        
        // Example: Give player experience
        // PlayerStats.AddExperience(100);
        
        Debug.Log("[ExampleIntegration] Custom puzzle completion logic executed");
    }
    
    private void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && teleportDestination != null)
        {
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;
            Debug.Log("[ExampleIntegration] Teleported player");
        }
    }
    
    // ==========================================
    // Public Methods for External Use
    // ==========================================
    
    /// <summary>
    /// Manually show the placeholder (useful for hints or tutorials)
    /// </summary>
    public void ShowPlaceholderHint()
    {
        if (headPlaceholder != null)
        {
            headPlaceholder.Show();
            headPlaceholder.PlayPulseAnimation();
            Debug.Log("[ExampleIntegration] Showing placeholder hint");
        }
    }
    
    /// <summary>
    /// Manually hide the placeholder
    /// </summary>
    public void HidePlaceholder()
    {
        if (headPlaceholder != null)
        {
            headPlaceholder.Hide();
            Debug.Log("[ExampleIntegration] Hiding placeholder");
        }
    }
    
    /// <summary>
    /// Flash the placeholder to draw attention
    /// </summary>
    public void FlashPlaceholder()
    {
        if (headPlaceholder != null)
        {
            headPlaceholder.Flash(Color.yellow, 0.5f);
            Debug.Log("[ExampleIntegration] Flashing placeholder");
        }
    }
    
    /// <summary>
    /// Check if player has the required head
    /// </summary>
    public bool PlayerHasCorrectHead()
    {
        DullahanHeadInventory inventory = FindObjectOfType<DullahanHeadInventory>();
        if (inventory != null && headPlacementPuzzle != null)
        {
            DullahanHeadSO currentHead = inventory.GetCurrentHead();
            if (currentHead != null)
            {
                return currentHead.headID == headPlacementPuzzle.requiredHeadID;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Reset the puzzle (useful for testing or game reset)
    /// </summary>
    public void ResetPuzzle()
    {
        if (headPlacementPuzzle != null)
        {
            headPlacementPuzzle.ResetPuzzle();
            puzzleWasCompleted = false;
            Debug.Log("[ExampleIntegration] Puzzle reset");
        }
    }
    
    /// <summary>
    /// Force complete the puzzle (testing only)
    /// </summary>
    public void ForceCompletePuzzle()
    {
        if (headPlacementPuzzle != null)
        {
            headPlacementPuzzle.ForceComplete();
            Debug.Log("[ExampleIntegration] Puzzle force completed");
        }
    }
    
    // ==========================================
    // Tutorial/Hint System Integration
    // ==========================================
    
    /// <summary>
    /// Example: Show tutorial message when player approaches
    /// </summary>
    public void ShowTutorialHint()
    {
        // Example integration with tutorial system
        // TutorialManager.ShowHint("Find the correct head and place it on the Dullahan's body");
        
        Debug.Log("[ExampleIntegration] Tutorial hint shown");
    }
    
    // ==========================================
    // Save/Load Integration
    // ==========================================
    
    /// <summary>
    /// Save puzzle state
    /// </summary>
    public void SavePuzzleState()
    {
        if (headPlacementPuzzle != null)
        {
            bool isCompleted = headPlacementPuzzle.IsPuzzleCompleted();
            // Save to your save system
            // SaveSystem.SetBool("DullahanHeadPuzzle_Completed", isCompleted);
            
            Debug.Log($"[ExampleIntegration] Saved puzzle state: {isCompleted}");
        }
    }
    
    /// <summary>
    /// Load puzzle state
    /// </summary>
    public void LoadPuzzleState()
    {
        // Load from your save system
        // bool wasCompleted = SaveSystem.GetBool("DullahanHeadPuzzle_Completed", false);
        
        // If puzzle was already completed, force complete it
        // if (wasCompleted && headPlacementPuzzle != null)
        // {
        //     headPlacementPuzzle.ForceComplete();
        // }
        
        Debug.Log("[ExampleIntegration] Loaded puzzle state");
    }
}


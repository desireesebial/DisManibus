using UnityEngine;
using System.Collections;

/// <summary>
/// Manages quest integration between Dullahan Chase Event and Head Shrine Puzzle
///
/// FUNCTIONALITY:
/// 1. Shows "Survive" quest during each chase phase with timer
/// 2. Shows "Find heads for exit" quest after first chase ends
/// 3. Tracks head placements in HeadShrinePuzzle
/// 4. Repeats survive quest for every chase cycle after 2nd head is placed
///
/// SETUP:
/// 1. Attach to any GameObject in the scene
/// 2. Assign DullahanChaseEventManager reference
/// 3. Assign HeadShrinePuzzle reference
/// 4. QuestManager is found automatically via singleton
/// 5. Configure quest text in Inspector
/// 6. Hook this script into DullahanChaseEventManager's Unity Events:
///    - OnChaseStart → DullahanQuestController.OnChaseStarted()
///    - OnChaseEnd → DullahanQuestController.OnChaseEnded()
///    - OnRestEnd → DullahanQuestController.OnRestEnded()
/// </summary>
public class DullahanQuestController : MonoBehaviour
{
    [Header("═══ REQUIRED REFERENCES ═══")]
    [Tooltip("Reference to the Dullahan Chase Event Manager")]
    public DullahanChaseEventManager chaseEventManager;

    [Tooltip("Reference to the Head Shrine Puzzle")]
    public HeadShrinePuzzle headShrinePuzzle;

    [Header("═══ SURVIVE QUEST SETTINGS ═══")]
    [Tooltip("Quest ID for survive quest (must be unique)")]
    public string surviveQuestId = "survive_dullahan";

    [Tooltip("Text shown during survive/chase phase")]
    [TextArea]
    public string surviveQuestText = "Survive the Dullahan's chase!";

    [Header("═══ FIND HEADS QUEST SETTINGS ═══")]
    [Tooltip("Quest ID for find heads quest (must be unique)")]
    public string findHeadsQuestId = "find_heads_for_exit";

    [Tooltip("Text shown when player needs to find heads")]
    [TextArea]
    public string findHeadsQuestText = "Find heads for the exit shrine";

    [Header("═══ BEHAVIOR SETTINGS ═══")]
    [Tooltip("Show survive quest on very first chase (before any heads placed)")]
    public bool showSurviveOnFirstChase = true;

    [Tooltip("Show find heads quest immediately after first chase ends")]
    public bool showFindHeadsAfterFirstChase = true;

    [Tooltip("Number of heads that must be placed before survive quest continues appearing")]
    public int headsRequiredForSurviveRepeat = 2;

    [Header("═══ DEBUG INFO (Read Only) ═══")]
    [SerializeField] private bool isFirstChaseDone = false;
    [SerializeField] private bool isFindHeadsQuestActive = false;
    [SerializeField] private bool isSurviveQuestActive = false;
    [SerializeField] private int lastKnownHeadCount = 0;

    // Private references
    private QuestManager questManager;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        // Find QuestManager
        questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("[DullahanQuestController] QuestManager not found! Make sure QuestManager exists in scene.");
            return;
        }

        // Validate references
        if (chaseEventManager == null)
        {
            Debug.LogWarning("[DullahanQuestController] DullahanChaseEventManager not assigned! Trying to find it...");
            chaseEventManager = FindObjectOfType<DullahanChaseEventManager>();

            if (chaseEventManager == null)
            {
                Debug.LogError("[DullahanQuestController] DullahanChaseEventManager not found in scene!");
                return;
            }
        }

        if (headShrinePuzzle == null)
        {
            Debug.LogWarning("[DullahanQuestController] HeadShrinePuzzle not assigned! Trying to find it...");
            headShrinePuzzle = FindObjectOfType<HeadShrinePuzzle>();

            if (headShrinePuzzle == null)
            {
                Debug.LogError("[DullahanQuestController] HeadShrinePuzzle not found in scene!");
                return;
            }
        }

        isInitialized = true;

        Debug.Log("[DullahanQuestController] Initialized successfully!");
        Debug.Log($"[DullahanQuestController] Survive Quest: '{surviveQuestText}'");
        Debug.Log($"[DullahanQuestController] Find Heads Quest: '{findHeadsQuestText}'");
    }

    void Update()
    {
        if (!isInitialized) return;

        // Monitor head placements
        UpdateHeadPlacementTracking();
    }

    /// <summary>
    /// Tracks head placements and updates find heads quest accordingly
    /// </summary>
    void UpdateHeadPlacementTracking()
    {
        if (headShrinePuzzle == null) return;

        int currentHeadCount = headShrinePuzzle.GetActivatedPlacementsCount();

        // Check if head count changed
        if (currentHeadCount != lastKnownHeadCount)
        {
            Debug.Log($"[DullahanQuestController] Head count changed: {lastKnownHeadCount} → {currentHeadCount}");
            lastKnownHeadCount = currentHeadCount;

            // Check if all heads are placed (complete find heads quest)
            if (isFindHeadsQuestActive && headShrinePuzzle.IsShrineComplete())
            {
                CompleteAndRemoveFindHeadsQuest();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API - Called by DullahanChaseEventManager Unity Events
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Called when chase phase starts (hook this to DullahanChaseEventManager.OnChaseStart)
    /// </summary>
    public void OnChaseStarted()
    {
        if (!isInitialized) return;

        Debug.Log("[DullahanQuestController] Chase started!");

        // Determine if we should show survive quest
        bool shouldShowSurvive = false;

        // First chase: show if enabled
        if (!isFirstChaseDone && showSurviveOnFirstChase)
        {
            shouldShowSurvive = true;
            Debug.Log("[DullahanQuestController] First chase - showing survive quest");
        }
        // Subsequent chases: show if enough heads placed
        else if (isFirstChaseDone && lastKnownHeadCount >= headsRequiredForSurviveRepeat)
        {
            shouldShowSurvive = true;
            Debug.Log($"[DullahanQuestController] {lastKnownHeadCount} heads placed (>= {headsRequiredForSurviveRepeat}) - showing survive quest");
        }

        // Show survive quest
        if (shouldShowSurvive)
        {
            ShowSurviveQuest();
        }
    }

    /// <summary>
    /// Called when chase phase ends (hook this to DullahanChaseEventManager.OnChaseEnd)
    /// </summary>
    public void OnChaseEnded()
    {
        if (!isInitialized) return;

        Debug.Log("[DullahanQuestController] Chase ended!");

        // Remove survive quest
        RemoveSurviveQuest();

        // Show find heads quest after first chase
        if (!isFirstChaseDone && showFindHeadsAfterFirstChase)
        {
            isFirstChaseDone = true;
            ShowFindHeadsQuest();
        }
    }

    /// <summary>
    /// Called when rest phase ends (hook this to DullahanChaseEventManager.OnRestEnd)
    /// Optional: Can be used for additional logic when rest period ends
    /// </summary>
    public void OnRestEnded()
    {
        if (!isInitialized) return;

        Debug.Log("[DullahanQuestController] Rest period ended!");

        // Note: OnChaseStarted() will be called next, which handles showing survive quest
        // This method is here for additional functionality if needed
    }

    // ═══════════════════════════════════════════════════════════
    // QUEST MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the survive quest
    /// </summary>
    void ShowSurviveQuest()
    {
        if (questManager == null) return;

        questManager.AddQuest(surviveQuestId, surviveQuestText);
        isSurviveQuestActive = true;

        Debug.Log($"[DullahanQuestController] ✓ Survive quest added: '{surviveQuestText}'");
    }

    /// <summary>
    /// Removes the survive quest (completes and removes it)
    /// </summary>
    void RemoveSurviveQuest()
    {
        if (questManager == null || !isSurviveQuestActive) return;

        // Complete quest (shows strike-through animation)
        questManager.CompleteQuest(surviveQuestId);
        isSurviveQuestActive = false;

        Debug.Log($"[DullahanQuestController] ✓ Survive quest completed");
    }

    /// <summary>
    /// Shows the find heads quest
    /// </summary>
    void ShowFindHeadsQuest()
    {
        if (questManager == null) return;

        // Don't show if shrine is already complete
        if (headShrinePuzzle != null && headShrinePuzzle.IsShrineComplete())
        {
            Debug.Log("[DullahanQuestController] Shrine already complete, skipping find heads quest");
            return;
        }

        questManager.AddQuest(findHeadsQuestId, findHeadsQuestText);
        isFindHeadsQuestActive = true;

        Debug.Log($"[DullahanQuestController] ✓ Find heads quest added: '{findHeadsQuestText}'");
    }

    /// <summary>
    /// Completes and removes the find heads quest
    /// </summary>
    void CompleteAndRemoveFindHeadsQuest()
    {
        if (questManager == null || !isFindHeadsQuestActive) return;

        questManager.CompleteQuest(findHeadsQuestId);
        isFindHeadsQuestActive = false;

        Debug.Log($"[DullahanQuestController] ✓ Find heads quest completed!");
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API - Manual Control (for testing/debugging)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Manually show survive quest (for testing)
    /// </summary>
    [ContextMenu("Test: Show Survive Quest")]
    public void TestShowSurviveQuest()
    {
        ShowSurviveQuest();
    }

    /// <summary>
    /// Manually remove survive quest (for testing)
    /// </summary>
    [ContextMenu("Test: Remove Survive Quest")]
    public void TestRemoveSurviveQuest()
    {
        RemoveSurviveQuest();
    }

    /// <summary>
    /// Manually show find heads quest (for testing)
    /// </summary>
    [ContextMenu("Test: Show Find Heads Quest")]
    public void TestShowFindHeadsQuest()
    {
        ShowFindHeadsQuest();
    }

    /// <summary>
    /// Manually complete find heads quest (for testing)
    /// </summary>
    [ContextMenu("Test: Complete Find Heads Quest")]
    public void TestCompleteFindHeadsQuest()
    {
        CompleteAndRemoveFindHeadsQuest();
    }

    /// <summary>
    /// Reset all quest states (for testing)
    /// </summary>
    [ContextMenu("Test: Reset All Quests")]
    public void ResetAllQuests()
    {
        isFirstChaseDone = false;
        isFindHeadsQuestActive = false;
        isSurviveQuestActive = false;
        lastKnownHeadCount = 0;

        if (questManager != null)
        {
            questManager.RemoveQuest(surviveQuestId);
            questManager.RemoveQuest(findHeadsQuestId);
        }

        Debug.Log("[DullahanQuestController] All quests reset!");
    }

    /// <summary>
    /// Print current state (for debugging)
    /// </summary>
    [ContextMenu("Debug: Print Status")]
    public void PrintStatus()
    {
        Debug.Log("═══ DULLAHAN QUEST CONTROLLER STATUS ═══");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"First Chase Done: {isFirstChaseDone}");
        Debug.Log($"Survive Quest Active: {isSurviveQuestActive}");
        Debug.Log($"Find Heads Quest Active: {isFindHeadsQuestActive}");
        Debug.Log($"Heads Placed: {lastKnownHeadCount} / {headsRequiredForSurviveRepeat}");

        if (headShrinePuzzle != null)
        {
            Debug.Log($"Shrine Complete: {headShrinePuzzle.IsShrineComplete()}");
            Debug.Log($"Total Placements: {headShrinePuzzle.GetTotalPlacementsCount()}");
        }

        Debug.Log("═══════════════════════════════════════");
    }
}

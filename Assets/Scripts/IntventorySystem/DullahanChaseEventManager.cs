using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DullahanChaseEventManager : MonoBehaviour
{
    [Header("Event Sequence Settings")]
    public float chaseDuration = 60f; // 1 minute chase
    public float doorOpenDelay = 2f; // Delay before door opens after chase
    public bool startChaseOnGameStart = true;
    
    [Header("Chase Phases")]
    public ChasePhase currentPhase = ChasePhase.Initial;
    public bool isChaseActive = false;
    public bool isEventComplete = false;
    
    [Header("Doors")]
    public Door[] phaseDoors; // Doors for each phase (Fake1, Fake2, Real)
    public int[] doorKeyIDs = { 201, 202, 203 }; // Key IDs for each door
    
    [Header("Timer UI")]
    public GameObject timerUI;
    public TextMeshProUGUI timerText;
    public Image timerFillImage;
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.red;
    public float warningThreshold = 10f; // Show warning when 10 seconds left
    
    [Header("Phase UI")]
    public TextMeshProUGUI phaseText;
    public string[] phaseNames = { "Initial Chase", "Fake Head 1", "Fake Head 2", "Real Head" };
    
    [Header("Integration")]
    public DullahanChaseSystem dullahanChaseSystem;
    public DullahanAudioManager audioManager;
    public DullahanHeadInventory headInventory;
    public DullahanBody dullahanBody;
    
    [Header("Debug")]
    public bool debugMode = false;
    public KeyCode skipPhaseKey = KeyCode.P;
    public KeyCode resetEventKey = KeyCode.R;
    
    private float currentChaseTime;
    private float maxChaseTime;
    private bool timerWarningPlayed = false;
    private bool phaseComplete = false;
    
    public enum ChasePhase
    {
        Initial,    // Start of game
        Fake1,      // After first chase, door to Fake Head 1
        Fake2,      // After second chase, door to Fake Head 2
        Real        // After third chase, door to Real Head
    }
    
    void Start()
    {
        InitializeEventManager();
        
        if (startChaseOnGameStart)
        {
            StartCoroutine(StartInitialChase());
        }
    }
    
    void Update()
    {
        if (!isEventComplete)
        {
            HandleChaseTimer();
            HandleDebugInput();
            UpdateUI();
        }
    }
    
    private void InitializeEventManager()
    {
        // Find references if not assigned
        if (dullahanChaseSystem == null)
            dullahanChaseSystem = FindObjectOfType<DullahanChaseSystem>();
            
        if (audioManager == null)
            audioManager = FindObjectOfType<DullahanAudioManager>();
            
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (dullahanBody == null)
            dullahanBody = FindObjectOfType<DullahanBody>();
        
        // Initialize timer
        maxChaseTime = chaseDuration;
        currentChaseTime = maxChaseTime;
        
        // Setup UI
        if (timerUI != null)
            timerUI.SetActive(false);
            
        // Lock all doors initially
        LockAllDoors();
        
        // Ensure Dullahan starts in patrol mode
        StartCoroutine(InitializePatrolMode());
        
        // Subscribe to head attachment events
        if (dullahanBody != null)
        {
            // We'll need to add an event to DullahanBody
        }
        
        Debug.Log("Dullahan Chase Event Manager initialized");
    }
    
    private IEnumerator InitializePatrolMode()
    {
        // Wait for all systems to initialize
        yield return new WaitForSeconds(1f);
        
        // Start Dullahan in patrol mode
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartPatrol();
        }
        
        Debug.Log("Dullahan initialized in patrol mode");
    }
    
    private IEnumerator StartInitialChase()
    {
        yield return new WaitForSeconds(2f); // Brief delay before first chase
        
        Debug.Log("Starting initial chase sequence...");
        StartChase();
    }
    
    public void StartChase()
    {
        if (isChaseActive) return;
        
        isChaseActive = true;
        currentChaseTime = maxChaseTime;
        timerWarningPlayed = false;
        phaseComplete = false;
        
        // Start chase system
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartChase();
        }
        
        // Start audio
        if (audioManager != null)
        {
            audioManager.StartChase();
        }
        
        // Show timer UI
        if (timerUI != null)
            timerUI.SetActive(true);
        
        Debug.Log($"Chase started for phase: {currentPhase}");
    }
    
    public void EndChase()
    {
        if (!isChaseActive) return;
        
        isChaseActive = false;
        
        // Stop chase system and return to patrol
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
        }
        
        // Stop audio
        if (audioManager != null)
        {
            audioManager.EndChase();
        }
        
        // Hide timer UI
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Start patrol mode
        StartCoroutine(StartPatrolAfterDelay());
        
        // Open door for next phase
        StartCoroutine(OpenPhaseDoor());
        
        Debug.Log($"Chase ended for phase: {currentPhase} - Dullahan returning to patrol");
    }
    
    private IEnumerator StartPatrolAfterDelay()
    {
        // Wait a moment before starting patrol
        yield return new WaitForSeconds(1f);
        
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartPatrol();
        }
        
        Debug.Log("Dullahan patrol mode activated");
    }
    
    private IEnumerator OpenPhaseDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);
        
        // Open door for current phase
        int doorIndex = GetDoorIndexForPhase(currentPhase);
        if (doorIndex >= 0 && doorIndex < phaseDoors.Length && phaseDoors[doorIndex] != null)
        {
            phaseDoors[doorIndex].UnlockDoor();
            Debug.Log($"Door opened for phase: {currentPhase}");
        }
        
        // Play door open sound
        if (audioManager != null)
        {
            audioManager.PlayDoorOpenSound();
        }
    }
    
    private void HandleChaseTimer()
    {
        if (!isChaseActive) return;
        
        currentChaseTime -= Time.deltaTime;
        
        // Check for warning
        if (currentChaseTime <= warningThreshold && !timerWarningPlayed)
        {
            timerWarningPlayed = true;
            if (audioManager != null)
            {
                audioManager.PlayTimerWarningSound();
            }
        }
        
        // Check if chase time is up
        if (currentChaseTime <= 0f)
        {
            currentChaseTime = 0f;
            EndChase();
        }
    }
    
    private void UpdateUI()
    {
        // Update timer text
        if (timerText != null && isChaseActive)
        {
            int minutes = Mathf.FloorToInt(currentChaseTime / 60f);
            int seconds = Mathf.FloorToInt(currentChaseTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Change color for warning
            if (currentChaseTime <= warningThreshold)
            {
                timerText.color = timerWarningColor;
            }
            else
            {
                timerText.color = timerNormalColor;
            }
        }
        
        // Update timer fill
        if (timerFillImage != null && isChaseActive)
        {
            timerFillImage.fillAmount = currentChaseTime / maxChaseTime;
        }
        
        // Update phase text
        if (phaseText != null)
        {
            int phaseIndex = (int)currentPhase;
            if (phaseIndex < phaseNames.Length)
            {
                phaseText.text = $"Phase: {phaseNames[phaseIndex]}";
            }
        }
    }
    
    public void OnHeadAttached(HeadType headType)
    {
        if (phaseComplete) return;
        
        Debug.Log($"Head attached: {headType}");
        
        // Check if this is the correct head for the current phase
        if (IsCorrectHeadForPhase(headType))
        {
            phaseComplete = true;
            
            // Progress to next phase
            ProgressToNextPhase();
            
            // Start next chase after a delay
            StartCoroutine(StartNextChase());
        }
        else
        {
            // Wrong head - play wrong head sound
            if (audioManager != null)
            {
                audioManager.PlayWrongHeadSound();
            }
            
            Debug.Log("Wrong head attached! Chase will continue...");
        }
    }
    
    private void ProgressToNextPhase()
    {
        switch (currentPhase)
        {
            case ChasePhase.Initial:
                currentPhase = ChasePhase.Fake1;
                break;
            case ChasePhase.Fake1:
                currentPhase = ChasePhase.Fake2;
                break;
            case ChasePhase.Fake2:
                currentPhase = ChasePhase.Real;
                break;
            case ChasePhase.Real:
                // Game complete!
                isEventComplete = true;
                OnGameComplete();
                return;
        }
        
        Debug.Log($"Progressed to phase: {currentPhase}");
    }
    
    private IEnumerator StartNextChase()
    {
        yield return new WaitForSeconds(3f); // Delay before next chase
        
        if (!isEventComplete)
        {
            StartChase();
        }
    }
    
    private bool IsCorrectHeadForPhase(HeadType headType)
    {
        switch (currentPhase)
        {
            case ChasePhase.Initial:
                return false; // No head should be attached in initial phase
            case ChasePhase.Fake1:
                return headType == HeadType.Fake1;
            case ChasePhase.Fake2:
                return headType == HeadType.Fake2;
            case ChasePhase.Real:
                return headType == HeadType.Real;
            default:
                return false;
        }
    }
    
    private int GetDoorIndexForPhase(ChasePhase phase)
    {
        switch (phase)
        {
            case ChasePhase.Initial:
                return 0; // Door to Fake Head 1
            case ChasePhase.Fake1:
                return 1; // Door to Fake Head 2
            case ChasePhase.Fake2:
                return 2; // Door to Real Head
            case ChasePhase.Real:
                return -1; // No door needed
            default:
                return -1;
        }
    }
    
    private void LockAllDoors()
    {
        for (int i = 0; i < phaseDoors.Length; i++)
        {
            if (phaseDoors[i] != null)
            {
                phaseDoors[i].LockDoor();
                if (i < doorKeyIDs.Length)
                {
                    phaseDoors[i].RequiredKeyID = doorKeyIDs[i];
                }
            }
        }
    }
    
    private void OnGameComplete()
    {
        Debug.Log("Dullahan puzzle completed! Game won!");
        
        // Play completion sound
        if (audioManager != null)
        {
            audioManager.PlayPuzzleCompleteSound();
        }
        
        // Hide timer UI
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // You can add game completion logic here
        // For example: Show victory screen, unlock achievements, etc.
    }
    
    private void HandleDebugInput()
    {
        if (!debugMode) return;
        
        if (Input.GetKeyDown(skipPhaseKey))
        {
            Debug.Log("Debug: Skipping current phase");
            EndChase();
        }
        
        if (Input.GetKeyDown(resetEventKey))
        {
            Debug.Log("Debug: Resetting event");
            ResetEvent();
        }
    }
    
    public void ResetEvent()
    {
        currentPhase = ChasePhase.Initial;
        isChaseActive = false;
        isEventComplete = false;
        phaseComplete = false;
        currentChaseTime = maxChaseTime;
        
        // Stop all systems and return to patrol
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
            // Ensure patrol mode is activated after reset
            StartCoroutine(StartPatrolAfterReset());
        }
            
        if (audioManager != null)
            audioManager.EndChase();
        
        // Hide UI
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Lock all doors
        LockAllDoors();
        
        Debug.Log("Event reset to initial state - Dullahan returning to patrol");
    }
    
    private IEnumerator StartPatrolAfterReset()
    {
        // Wait a moment before starting patrol after reset
        yield return new WaitForSeconds(2f);
        
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartPatrol();
        }
        
        Debug.Log("Dullahan patrol mode activated after reset");
    }
    
    // Public methods for external control
    public void SetChaseDuration(float duration)
    {
        chaseDuration = duration;
        maxChaseTime = duration;
        Debug.Log($"Chase duration set to {duration} seconds");
    }
    
    public void SetWarningThreshold(float threshold)
    {
        warningThreshold = threshold;
        Debug.Log($"Warning threshold set to {threshold} seconds");
    }
    
    public float GetCurrentChaseTime()
    {
        return currentChaseTime;
    }
    
    public float GetChaseProgress()
    {
        return 1f - (currentChaseTime / maxChaseTime);
    }
    
    public bool IsChaseActive()
    {
        return isChaseActive;
    }
    
    public ChasePhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    public bool IsEventComplete()
    {
        return isEventComplete;
    }
}

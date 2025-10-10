using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Floor2EndingEventManager : MonoBehaviour
{
    [Header("Event Settings")]
    public bool eventActive = true;
    public float proximityRadius = 15f;
    public Transform eventTriggerPoint; // Point where the event starts
    
    [Header("Choice System")]
    public bool playerChoiceMade = false;
    public bool playerChoseToHelp = false;
    public float choiceTimeLimit = 45f; // Time to make choice
    private float choiceTimer = 0f;
    
    [Header("Doors")]
    public doorscript exitDoor; // Exit to Floor 1 (final level)
    public doorscript realHeadDoor; // Door to real head room (good ending path)
    public int exitDoorKeyID = 999;
    public int realHeadDoorKeyID = 100;
    
    [Header("Event States")]
    public EventState currentState = EventState.Waiting;
    public bool isEventComplete = false;
    public bool badEndingTriggered = false;
    public bool goodEndingTriggered = false;
    
    [Header("Timer UI")]
    public GameObject timerUI;
    public TextMeshProUGUI timerText;
    public Image timerFillImage;
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.red;
    public float warningThreshold = 10f;
    
    [Header("Choice UI")]
    public GameObject choiceUI;
    public TextMeshProUGUI choiceText;
    public Button helpButton;
    public Button leaveButton;
    public string helpText = "Help Dullahan find his real head?";
    public string leaveText = "Leave through exit door to Floor 1";
    public string choicePromptText = "You have found yourself in Dullahan's territory. What will you do?";
    
    [Header("Proximity UI")]
    public GameObject proximityUI;
    public TextMeshProUGUI proximityText;
    public string proximityWarningText = "Dullahan's territory... Choose your path carefully.";
    
    [Header("Integration")]
    public DullahanHeadInventory headInventory;
    public DullahanBody dullahanBody;
    public DullahanPuzzleManager puzzleManager;
    public DullahanChaseEventManager chaseEventManager;
    public QuestSystem questSystem;
    
    [Header("Scene Management")]
    public string floor1SceneName = "Floor1";
    public string badEndingSceneName = "BadEnding";
    public string goodEndingSceneName = "GoodEnding";
    
    [Header("Quest Integration")]
    public Quest helpDullahanQuest;
    public Quest escapeQuest;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip choiceMusic;
    public AudioClip badEndingMusic;
    public AudioClip goodEndingMusic;
    
    [Header("Debug")]
    public bool debugMode = false;
    public KeyCode triggerEventKey = KeyCode.P;
    public KeyCode forceChoiceKey = KeyCode.Space;
    
    private float currentTimer;
    private float maxTimer;
    private bool timerWarningPlayed = false;
    private Transform playerTransform;
    private bool playerInProximity = false;
    private bool proximityTriggered = false;
    
    public enum EventState
    {
        Waiting,        // Waiting for player to enter proximity
        Proximity,      // Player in proximity, showing warning
        Choice,         // Player making choice
        BadEnding,      // Player chose to leave directly
        GoodEnding,     // Player chose to help Dullahan
        Completion      // Event complete
    }
    
    void Start()
    {
        InitializeEventManager();
    }
    
    void Update()
    {
        if (!eventActive || isEventComplete) return;
        
        HandleProximityDetection();
        HandleCurrentState();
        HandleDebugInput();
        UpdateUI();
    }
    
    private void InitializeEventManager()
    {
        // Find references if not assigned
        if (headInventory == null)
            headInventory = FindObjectOfType<DullahanHeadInventory>();
            
        if (dullahanBody == null)
            dullahanBody = FindObjectOfType<DullahanBody>();
            
        if (puzzleManager == null)
            puzzleManager = FindObjectOfType<DullahanPuzzleManager>();
            
        if (chaseEventManager == null)
            chaseEventManager = FindObjectOfType<DullahanChaseEventManager>();
            
        if (questSystem == null)
            questSystem = FindObjectOfType<QuestSystem>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        // Setup event trigger point if not assigned
        if (eventTriggerPoint == null)
        {
            eventTriggerPoint = transform;
        }
        
        // Find doors if not assigned
        FindMissingDoors();
        
        // Initialize timers
        maxTimer = choiceTimeLimit;
        currentTimer = maxTimer;
        
        // Setup UI
        if (timerUI != null)
            timerUI.SetActive(false);
        if (choiceUI != null)
            choiceUI.SetActive(false);
        if (proximityUI != null)
            proximityUI.SetActive(false);
            
        // Lock doors initially
        LockAllDoors();
        
        // Setup choice buttons
        SetupChoiceButtons();
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        Debug.Log("Floor 2 Ending Event Manager initialized - Waiting for player proximity");
    }
    
    private void SetupChoiceButtons()
    {
        if (helpButton != null)
        {
            helpButton.onClick.AddListener(OnHelpChosen);
        }
        
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveChosen);
        }
    }
    
    private void HandleProximityDetection()
    {
        if (proximityTriggered || playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(eventTriggerPoint.position, playerTransform.position);
        bool wasInProximity = playerInProximity;
        playerInProximity = distanceToPlayer <= proximityRadius;
        
        // Player just entered proximity
        if (playerInProximity && !wasInProximity)
        {
            OnPlayerEnteredProximity();
        }
        // Player left proximity
        else if (!playerInProximity && wasInProximity)
        {
            OnPlayerLeftProximity();
        }
    }
    
    private void OnPlayerEnteredProximity()
    {
        if (proximityTriggered) return;
        
        Debug.Log("Player entered Floor 2 ending event proximity!");
        proximityTriggered = true;
        currentState = EventState.Proximity;
        
        // Show proximity warning
        if (proximityUI != null)
        {
            proximityUI.SetActive(true);
            if (proximityText != null)
                proximityText.text = proximityWarningText;
        }
        
        // Start choice sequence after brief delay
        StartCoroutine(StartChoiceSequence());
    }
    
    private void OnPlayerLeftProximity()
    {
        // Only relevant if player hasn't triggered the sequence yet
        if (!proximityTriggered)
        {
            if (proximityUI != null)
                proximityUI.SetActive(false);
        }
    }
    
    private IEnumerator StartChoiceSequence()
    {
        yield return new WaitForSeconds(3f); // Brief warning delay
        
        // Hide proximity UI
        if (proximityUI != null)
            proximityUI.SetActive(false);
        
        // Show choice UI
        ShowChoiceUI();
    }
    
    private void HandleCurrentState()
    {
        switch (currentState)
        {
            case EventState.Waiting:
                // Waiting for proximity trigger
                break;
                
            case EventState.Proximity:
                // Player in proximity, showing warning
                break;
                
            case EventState.Choice:
                HandleChoiceTimer();
                break;
                
            case EventState.BadEnding:
                // Bad ending path chosen
                break;
                
            case EventState.GoodEnding:
                // Good ending path chosen
                break;
                
            case EventState.Completion:
                // Event complete
                break;
        }
    }
    
    private void HandleChoiceTimer()
    {
        if (playerChoiceMade) return;
        
        choiceTimer += Time.deltaTime;
        currentTimer = maxTimer - choiceTimer;
        
        // Check for warning
        if (currentTimer <= warningThreshold && !timerWarningPlayed)
        {
            timerWarningPlayed = true;
            Debug.Log("Choice time running out!");
        }
        
        // Check if time is up
        if (choiceTimer >= choiceTimeLimit)
        {
            // Auto-choose to leave if no choice made
            OnLeaveChosen();
        }
    }
    
    private void ShowChoiceUI()
    {
        currentState = EventState.Choice;
        choiceTimer = 0f;
        currentTimer = maxTimer;
        timerWarningPlayed = false;
        
        if (choiceUI != null)
        {
            choiceUI.SetActive(true);
            if (choiceText != null)
                choiceText.text = choicePromptText;
        }
        
        // Show timer UI
        if (timerUI != null)
            timerUI.SetActive(true);
        
        // Play choice music
        if (audioSource != null && choiceMusic != null)
        {
            audioSource.clip = choiceMusic;
            audioSource.Play();
        }
        
        Debug.Log("Player choice UI shown - 45 seconds to decide");
    }
    
    private void OnHelpChosen()
    {
        if (playerChoiceMade) return;
        
        playerChoiceMade = true;
        playerChoseToHelp = true;
        
        Debug.Log("Player chose to help Dullahan - Good ending path set for after Floor 1");
        
        // Hide choice UI
        if (choiceUI != null)
            choiceUI.SetActive(false);
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Start good ending path
        StartGoodEndingPath();
    }
    
    private void OnLeaveChosen()
    {
        if (playerChoiceMade) return;
        
        playerChoiceMade = true;
        playerChoseToHelp = false;
        
        Debug.Log("Player chose to leave directly - Bad ending path set for after Floor 1");
        
        // Hide choice UI
        if (choiceUI != null)
            choiceUI.SetActive(false);
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Start bad ending path
        StartBadEndingPath();
    }
    
    private void StartGoodEndingPath()
    {
        currentState = EventState.GoodEnding;
        goodEndingTriggered = true;
        
        // Save the choice for the ending after Floor 1
        SaveEndingChoice(true);
        
        // Start quest for helping Dullahan
        if (questSystem != null && helpDullahanQuest != null)
        {
            questSystem.StartQuest(helpDullahanQuest);
        }
        
        // Open real head door
        if (realHeadDoor != null)
        {
            realHeadDoor.ForceUnlock();
            realHeadDoor.OpenDoor();
            Debug.Log("Real head door opened for good ending path");
        }
        
        // Activate Dullahan chase event manager if available
        if (chaseEventManager != null)
        {
            chaseEventManager.playerChoseToHelp = true;
            chaseEventManager.choiceMade = true;
        }
        
        // Activate puzzle manager
        if (puzzleManager != null)
        {
            puzzleManager.puzzleActive = true;
        }
        
        // Open exit door to Floor 1 (final level)
        if (exitDoor != null)
        {
            exitDoor.ForceUnlock();
            exitDoor.OpenDoor();
            Debug.Log("Exit door opened - Player can proceed to Floor 1 (final level)");
        }
        
        Debug.Log("Good ending path activated - Player must complete Floor 1 to get good ending");
    }
    
    private void StartBadEndingPath()
    {
        currentState = EventState.BadEnding;
        badEndingTriggered = true;
        
        // Save the choice for the ending after Floor 1
        SaveEndingChoice(false);
        
        // Start escape quest
        if (questSystem != null && escapeQuest != null)
        {
            questSystem.StartQuest(escapeQuest);
        }
        
        // Open exit door to Floor 1 (final level)
        if (exitDoor != null)
        {
            exitDoor.ForceUnlock();
            exitDoor.OpenDoor();
            Debug.Log("Exit door opened - Player can proceed to Floor 1 (final level)");
        }
        
        // Play ominous music to indicate bad ending path
        if (audioSource != null && badEndingMusic != null)
        {
            audioSource.clip = badEndingMusic;
            audioSource.Play();
        }
        
        Debug.Log("Bad ending path activated - Player must complete Floor 1 to get bad ending");
    }
    
    private void SaveEndingChoice(bool choseGoodEnding)
    {
        // Save the player's choice to PlayerPrefs so it persists across scenes
        PlayerPrefs.SetInt("Floor2EndingChoice", choseGoodEnding ? 1 : 0);
        PlayerPrefs.SetString("Floor2EndingChoiceTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();
        
        Debug.Log($"Ending choice saved: {(choseGoodEnding ? "Good Ending" : "Bad Ending")} will occur after Floor 1 completion");
    }
    
    public void OnRealHeadAttached()
    {
        // Allow head attachment in GoodEnding state OR if event hasn't been triggered yet
        // This allows the puzzle to work even if Floor2EndingEventManager isn't being used
        if (currentState != EventState.GoodEnding && currentState != EventState.Waiting && eventActive) 
        {
            Debug.LogWarning($"Floor2EndingEventManager: Cannot attach real head in state {currentState}. Expected GoodEnding state.");
            return;
        }
        
        Debug.Log("Real head attached - Good ending path confirmed!");
        
        currentState = EventState.Completion;
        isEventComplete = true;
        
        // Complete help Dullahan quest
        if (questSystem != null && helpDullahanQuest != null)
        {
            questSystem.CompleteQuest(helpDullahanQuest);
        }
        
        // Play good ending music
        if (audioSource != null && goodEndingMusic != null)
        {
            audioSource.clip = goodEndingMusic;
            audioSource.Play();
        }
        
        // Open exit door to Floor 1 (final level)
        if (exitDoor != null)
        {
            exitDoor.ForceUnlock();
            exitDoor.OpenDoor();
            Debug.Log("Exit door opened - Player can proceed to Floor 1 (final level)");
        }
        
        Debug.Log("Good ending path confirmed - Player must complete Floor 1 to get good ending");
    }
    
    private void UpdateUI()
    {
        // Update timer text
        if (timerText != null && currentState == EventState.Choice)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Change color for warning
            if (currentTimer <= warningThreshold)
            {
                timerText.color = timerWarningColor;
            }
            else
            {
                timerText.color = timerNormalColor;
            }
        }
        
        // Update timer fill
        if (timerFillImage != null && currentState == EventState.Choice)
        {
            timerFillImage.fillAmount = currentTimer / maxTimer;
        }
        
        // Update choice timer
        if (currentState == EventState.Choice && choiceText != null)
        {
            float remainingChoiceTime = choiceTimeLimit - choiceTimer;
            choiceText.text = $"{choicePromptText}\nTime remaining: {remainingChoiceTime:F1}s";
        }
    }
    
    private void LockAllDoors()
    {
        // Lock exit door
        if (exitDoor != null)
        {
            exitDoor.LockDoor();
            exitDoor.SetRequiredKeyID(exitDoorKeyID);
        }
        
        // Lock real head door
        if (realHeadDoor != null)
        {
            realHeadDoor.LockDoor();
            realHeadDoor.SetRequiredKeyID(realHeadDoorKeyID);
        }
    }
    
    private void HandleDebugInput()
    {
        if (!debugMode) return;
        
        if (Input.GetKeyDown(triggerEventKey))
        {
            Debug.Log("Debug: Triggering event");
            OnPlayerEnteredProximity();
        }
        
        if (Input.GetKeyDown(forceChoiceKey))
        {
            Debug.Log("Debug: Forcing choice");
            if (currentState == EventState.Choice)
            {
                OnHelpChosen();
            }
        }
    }
    
    // Door management methods
    private void FindMissingDoors()
    {
        // Find exit door if not assigned
        if (exitDoor == null)
        {
            doorscript[] allDoors = FindObjectsOfType<doorscript>();
            foreach (var door in allDoors)
            {
                if (door.name.ToLower().Contains("exit") || door.name.ToLower().Contains("floor1"))
                {
                    exitDoor = door;
                    Debug.Log("Found exit door automatically");
                    break;
                }
            }
        }
        
        // Find real head door if not assigned
        if (realHeadDoor == null)
        {
            doorscript[] allDoors = FindObjectsOfType<doorscript>();
            foreach (var door in allDoors)
            {
                if (door.name.ToLower().Contains("real") || door.name.ToLower().Contains("head"))
                {
                    realHeadDoor = door;
                    Debug.Log("Found real head door automatically");
                    break;
                }
            }
        }
    }
    
    // Public methods for external control
    public void SetProximityRadius(float radius)
    {
        proximityRadius = radius;
        Debug.Log($"Proximity radius set to {radius}");
    }
    
    public void SetChoiceTimeLimit(float time)
    {
        choiceTimeLimit = time;
        maxTimer = time;
        Debug.Log($"Choice time limit set to {time} seconds");
    }
    
    public EventState GetCurrentState()
    {
        return currentState;
    }
    
    public bool IsEventComplete()
    {
        return isEventComplete;
    }
    
    public bool IsBadEndingTriggered()
    {
        return badEndingTriggered;
    }
    
    public bool IsGoodEndingTriggered()
    {
        return goodEndingTriggered;
    }
    
    public bool HasPlayerChosenToHelp()
    {
        return playerChoseToHelp;
    }
    
    // Method to be called by DullahanBody when real head is attached
    public void OnRealHeadAttachedToBody()
    {
        OnRealHeadAttached();
    }
    
    // Static method to check ending choice from other scenes
    public static bool GetEndingChoice()
    {
        return PlayerPrefs.GetInt("Floor2EndingChoice", 0) == 1;
    }
    
    // Static method to clear ending choice (useful for new game)
    public static void ClearEndingChoice()
    {
        PlayerPrefs.DeleteKey("Floor2EndingChoice");
        PlayerPrefs.DeleteKey("Floor2EndingChoiceTime");
        PlayerPrefs.Save();
    }
    
    public void ResetEvent()
    {
        // Reset the entire event system
        currentState = EventState.Waiting;
        isEventComplete = false;
        badEndingTriggered = false;
        goodEndingTriggered = false;
        playerInProximity = false;
        proximityTriggered = false;
        playerChoseToHelp = false;
        playerChoiceMade = false;
        choiceTimer = 0f;
        currentTimer = 0f;
        maxTimer = 0f;
        timerWarningPlayed = false;
        
        // Reset UI
        if (timerUI != null) timerUI.SetActive(false);
        if (choiceUI != null) choiceUI.SetActive(false);
        if (proximityUI != null) proximityUI.SetActive(false);
        
        // Reset doors
        LockAllDoors();
        
        Debug.Log("Floor 2 ending event system reset");
    }
}

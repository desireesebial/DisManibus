using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DullahanChaseEventManager : MonoBehaviour
{
    [Header("Proximity Settings")]
    public float proximityRadius = 10f;
    public Transform proximityCenter; // Center point for proximity detection
    public bool playerInProximity = false;
    public bool proximityTriggered = false;
    
    [Header("Chase Settings")]
    public float chaseDuration = 60f; // 60 seconds chase
    public float headSpawnDelay = 2f; // Delay before heads spawn after chase
    public float headCollectionTime = 90f; // 90 seconds to collect heads
    
    [Header("Doors")]
    public doorscript[] exitDoors; // Multiple exit doors that open together after chase
    public doorscript realHeadDoor; // Door to real head room
    public int exitDoorKeyID = 999;
    public int realHeadDoorKeyID = 100;
    public bool exitDoorsLinked = true; // Whether exit doors should open simultaneously
    
    [Header("Player Choice")]
    public bool playerChoseToHelp = false;
    public bool choiceMade = false;
    public float choiceTimeLimit = 30f; // Time to make choice
    private float choiceTimer = 0f;
    
    [Header("Game States")]
    public GameState currentState = GameState.Waiting;
    public bool isChaseActive = false;
    public bool isEventComplete = false;
    public bool badEndingTriggered = false;
    
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
    public string helpText = "Help Dullahan find its head?";
    public string leaveText = "Leave through exit door";
    
    [Header("Proximity UI")]
    public GameObject proximityUI;
    public TextMeshProUGUI proximityText;
    public string proximityWarningText = "Dullahan's territory...";
    
    [Header("Integration")]
    public DullahanChaseSystem dullahanChaseSystem;
    public DullahanAudioManager audioManager;
    public DullahanHeadInventory headInventory;
    public DullahanBody dullahanBody;
    public DullahanPuzzleManager puzzleManager;
    
    [Header("Scene Management")]
    public string nextSceneName = "NextLevel";
    public string badEndingSceneName = "BadEnding";
    
    [Header("Debug")]
    public bool debugMode = false;
    public KeyCode triggerProximityKey = KeyCode.P;
    public KeyCode skipChaseKey = KeyCode.C;
    public KeyCode forceChoiceKey = KeyCode.Space;
    
    private float currentTimer;
    private float maxTimer;
    private bool timerWarningPlayed = false;
    private Transform playerTransform;
    
    public enum GameState
    {
        Waiting,        // Waiting for player to enter proximity
        Chase,          // Dullahan chasing player
        Choice,         // Player making choice
        HeadCollection, // Player collecting heads
        Completion,     // Real head attached, game complete
        BadEnding       // Player chose to leave
    }
    
    void Start()
    {
        InitializeEventManager();
    }
    
    void Update()
    {
        if (!isEventComplete && !badEndingTriggered)
        {
            HandleProximityDetection();
            HandleCurrentState();
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
            
        if (puzzleManager == null)
            puzzleManager = FindObjectOfType<DullahanPuzzleManager>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        // Setup proximity center if not assigned
        if (proximityCenter == null)
        {
            proximityCenter = transform;
        }
        
        // Find doors if not assigned
        FindMissingDoors();
        
        // Initialize timers
        maxTimer = chaseDuration;
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
        
        // Ensure Dullahan starts in patrol mode
        StartCoroutine(InitializePatrolMode());
        
        // Setup choice buttons
        SetupChoiceButtons();
        
        Debug.Log("Dullahan Chase Event Manager initialized - Waiting for proximity trigger");
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
    
    private IEnumerator InitializePatrolMode()
    {
        yield return new WaitForSeconds(1f);
        
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartPatrol();
        }
        
        Debug.Log("Dullahan initialized in patrol mode");
    }
    
    private void HandleProximityDetection()
    {
        if (proximityTriggered || playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(proximityCenter.position, playerTransform.position);
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
        
        Debug.Log("Player entered Dullahan's proximity - Chase sequence starting!");
        proximityTriggered = true;
        
        // Show proximity warning
        if (proximityUI != null)
        {
            proximityUI.SetActive(true);
            if (proximityText != null)
                proximityText.text = proximityWarningText;
        }
        
        // Start chase after brief delay
        StartCoroutine(StartChaseSequence());
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
    
    private IEnumerator StartChaseSequence()
    {
        yield return new WaitForSeconds(2f); // Brief warning delay
        
        // Hide proximity UI
        if (proximityUI != null)
            proximityUI.SetActive(false);
        
        // Start chase
        StartChase();
    }
    
    private void HandleCurrentState()
    {
        switch (currentState)
        {
            case GameState.Waiting:
                // Waiting for proximity trigger
                break;
                
            case GameState.Chase:
                HandleChaseTimer();
                break;
                
            case GameState.Choice:
                HandleChoiceTimer();
                break;
                
            case GameState.HeadCollection:
                HandleHeadCollectionTimer();
                break;
                
            case GameState.Completion:
                // Waiting for real head attachment
                break;
                
            case GameState.BadEnding:
                // Bad ending triggered
                break;
        }
    }
    
    private void HandleChaseTimer()
    {
        if (!isChaseActive) return;
        
        currentTimer -= Time.deltaTime;
        
        // Check for warning
        if (currentTimer <= warningThreshold && !timerWarningPlayed)
        {
            timerWarningPlayed = true;
            if (audioManager != null)
            {
                audioManager.PlayTimerWarningSound();
            }
        }
        
        // Check if chase time is up
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            EndChase();
        }
    }
    
    private void HandleChoiceTimer()
    {
        if (choiceMade) return;
        
        choiceTimer += Time.deltaTime;
        
        if (choiceTimer >= choiceTimeLimit)
        {
            // Auto-choose to leave if no choice made
            OnLeaveChosen();
        }
    }
    
    private void HandleHeadCollectionTimer()
    {
        currentTimer -= Time.deltaTime;
        
        // Check for warning
        if (currentTimer <= warningThreshold && !timerWarningPlayed)
        {
            timerWarningPlayed = true;
            if (audioManager != null)
            {
                audioManager.PlayTimerWarningSound();
            }
        }
        
        // Check if time is up
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            OnHeadCollectionTimeUp();
        }
    }
    
    private void StartChase()
    {
        if (isChaseActive) return;
        
        currentState = GameState.Chase;
        isChaseActive = true;
        currentTimer = maxTimer;
        timerWarningPlayed = false;
        
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
        
        Debug.Log("Dullahan chase started - 60 seconds remaining");
    }
    
    private void EndChase()
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
        
        // Open exit doors (bait)
        StartCoroutine(OpenExitDoors());
        
        // Show choice UI
        StartCoroutine(ShowChoiceUI());
        
        Debug.Log("Chase ended - Exit door opened, waiting for player choice");
    }
    
    private IEnumerator StartPatrolAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartPatrol();
        }
        
        Debug.Log("Dullahan patrol mode activated");
    }
    
    private IEnumerator OpenExitDoors()
    {
        yield return new WaitForSeconds(2f);
        
        // Open all exit doors simultaneously
        if (exitDoors != null && exitDoors.Length > 0)
        {
            if (exitDoorsLinked && exitDoors.Length > 1)
            {
                // Link the doors so they open together
                for (int i = 0; i < exitDoors.Length; i++)
                {
                    if (exitDoors[i] != null)
                    {
                        // Create linked doors array excluding current door
                        doorscript[] linkedDoors = new doorscript[exitDoors.Length - 1];
                        int linkedIndex = 0;
                        for (int j = 0; j < exitDoors.Length; j++)
                        {
                            if (j != i)
                            {
                                linkedDoors[linkedIndex] = exitDoors[j];
                                linkedIndex++;
                            }
                        }
                        exitDoors[i].linkedDoors = linkedDoors;
                    }
                }
            }
            
            // Open the first door (others will open automatically if linked)
            if (exitDoors[0] != null)
            {
                exitDoors[0].ForceUnlock();
                exitDoors[0].OpenDoor();
                Debug.Log($"Exit doors opened simultaneously (bait)");
            }
        }
        
        // Play door open sound
        if (audioManager != null)
        {
            audioManager.PlayDoorOpenSound();
        }
    }
    
    private IEnumerator ShowChoiceUI()
    {
        yield return new WaitForSeconds(3f); // Delay before showing choice
        
        currentState = GameState.Choice;
        choiceTimer = 0f;
        
        if (choiceUI != null)
        {
            choiceUI.SetActive(true);
            if (choiceText != null)
                choiceText.text = helpText;
        }
        
        Debug.Log("Player choice UI shown - 30 seconds to decide");
    }
    
    private void OnHelpChosen()
    {
        if (choiceMade) return;
        
        choiceMade = true;
        playerChoseToHelp = true;
        
        Debug.Log("Player chose to help Dullahan");
        
        // Hide choice UI
        if (choiceUI != null)
            choiceUI.SetActive(false);
        
        // Start head collection phase
        StartCoroutine(StartHeadCollectionPhase());
    }
    
    private void OnLeaveChosen()
    {
        if (choiceMade) return;
        
        choiceMade = true;
        playerChoseToHelp = false;
        
        Debug.Log("Player chose to leave - Bad ending triggered");
        
        // Hide choice UI
        if (choiceUI != null)
            choiceUI.SetActive(false);
        
        // Trigger bad ending
        TriggerBadEnding();
    }
    
    private IEnumerator StartHeadCollectionPhase()
    {
        yield return new WaitForSeconds(headSpawnDelay);
        
        currentState = GameState.HeadCollection;
        currentTimer = headCollectionTime;
        maxTimer = headCollectionTime;
        timerWarningPlayed = false;
        
        // Spawn all three heads
        if (puzzleManager != null)
        {
            puzzleManager.SpawnAllHeads();
        }
        
        // Open door to real head
        if (realHeadDoor != null)
        {
            realHeadDoor.ForceUnlock();
            realHeadDoor.OpenDoor();
            Debug.Log("Real head door opened");
        }
        
        // Show timer UI
        if (timerUI != null)
            timerUI.SetActive(true);
        
        Debug.Log("Head collection phase started - 90 seconds to find real head");
    }
    
    private void OnHeadCollectionTimeUp()
    {
        Debug.Log("Head collection time expired - Bad ending");
        
        // Hide timer UI
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Trigger bad ending
        TriggerBadEnding();
    }
    
    public void OnRealHeadAttached()
    {
        if (currentState != GameState.HeadCollection) return;
        
        Debug.Log("Real head attached - Good ending!");
        
        currentState = GameState.Completion;
        
        // Stop Dullahan
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
        }
        
        // Hide timer UI
        if (timerUI != null)
            timerUI.SetActive(false);
        
        // Play completion sound
        if (audioManager != null)
        {
            audioManager.PlayPuzzleCompleteSound();
        }
        
        // Open exit doors for good ending
        StartCoroutine(OpenGoodEndingDoors());
        
        isEventComplete = true;
    }
    
    private IEnumerator OpenGoodEndingDoors()
    {
        yield return new WaitForSeconds(2f);
        
        // Open all exit doors for good ending
        if (exitDoors != null && exitDoors.Length > 0)
        {
            if (exitDoorsLinked && exitDoors.Length > 1)
            {
                // Link the doors so they open together
                for (int i = 0; i < exitDoors.Length; i++)
                {
                    if (exitDoors[i] != null)
                    {
                        // Create linked doors array excluding current door
                        doorscript[] linkedDoors = new doorscript[exitDoors.Length - 1];
                        int linkedIndex = 0;
                        for (int j = 0; j < exitDoors.Length; j++)
                        {
                            if (j != i)
                            {
                                linkedDoors[linkedIndex] = exitDoors[j];
                                linkedIndex++;
                            }
                        }
                        exitDoors[i].linkedDoors = linkedDoors;
                    }
                }
            }
            
            // Open the first door (others will open automatically if linked)
            if (exitDoors[0] != null)
            {
                exitDoors[0].ForceUnlock();
                exitDoors[0].OpenDoor();
                Debug.Log($"Exit doors opened simultaneously for good ending");
            }
        }
        
        // Play door open sound
        if (audioManager != null)
        {
            audioManager.PlayDoorOpenSound();
        }
    }
    
    private void TriggerBadEnding()
    {
        currentState = GameState.BadEnding;
        badEndingTriggered = true;
        
        Debug.Log("Bad ending triggered - Dullahan will kill the player");
        
        // Start aggressive chase
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartChase();
            // Make chase more intense for bad ending
            dullahanChaseSystem.SetChaseSpeed(8f, 12f);
        }
        
        // Play bad ending audio
        if (audioManager != null)
        {
            audioManager.StartChase();
        }
        
        // Load bad ending scene after delay
        StartCoroutine(LoadBadEndingScene());
    }
    
    private IEnumerator LoadBadEndingScene()
    {
        yield return new WaitForSeconds(5f); // Give player time to experience the bad ending
        
        if (!string.IsNullOrEmpty(badEndingSceneName))
        {
            SceneManager.LoadScene(badEndingSceneName);
        }
    }
    
    private void UpdateUI()
    {
        // Update timer text
        if (timerText != null && (isChaseActive || currentState == GameState.HeadCollection))
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
        if (timerFillImage != null && (isChaseActive || currentState == GameState.HeadCollection))
        {
            timerFillImage.fillAmount = currentTimer / maxTimer;
        }
        
        // Update choice timer
        if (currentState == GameState.Choice && choiceText != null)
        {
            float remainingChoiceTime = choiceTimeLimit - choiceTimer;
            choiceText.text = $"{helpText}\nTime remaining: {remainingChoiceTime:F1}s";
        }
    }
    
    private void LockAllDoors()
    {
        // Lock all exit doors
        if (exitDoors != null && exitDoors.Length > 0)
        {
            foreach (var exitDoor in exitDoors)
            {
                if (exitDoor != null)
                {
                    exitDoor.LockDoor();
                    exitDoor.SetRequiredKeyID(exitDoorKeyID);
                }
            }
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
        
        if (Input.GetKeyDown(triggerProximityKey))
        {
            Debug.Log("Debug: Triggering proximity");
            OnPlayerEnteredProximity();
        }
        
        if (Input.GetKeyDown(skipChaseKey))
        {
            Debug.Log("Debug: Skipping chase");
            EndChase();
        }
        
        if (Input.GetKeyDown(forceChoiceKey))
        {
            Debug.Log("Debug: Forcing choice");
            if (currentState == GameState.Choice)
            {
                OnHelpChosen();
            }
        }
    }
    
    // Public methods for external control
    public void SetProximityRadius(float radius)
    {
        proximityRadius = radius;
        Debug.Log($"Proximity radius set to {radius}");
    }
    
    public void SetChaseDuration(float duration)
    {
        chaseDuration = duration;
        maxTimer = duration;
        Debug.Log($"Chase duration set to {duration} seconds");
    }
    
    public void SetHeadCollectionTime(float time)
    {
        headCollectionTime = time;
        Debug.Log($"Head collection time set to {time} seconds");
    }
    
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    public bool IsChaseActive()
    {
        return isChaseActive;
    }
    
    public bool IsEventComplete()
    {
        return isEventComplete;
    }
    
    public bool IsBadEndingTriggered()
    {
        return badEndingTriggered;
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
    
    public void OnHeadAttached(HeadType headType)
    {
        // Handle head attachment events
        if (headType == HeadType.Real)
        {
            OnRealHeadAttachedToBody();
        }
        else
        {
            // Handle fake head attachments if needed
            Debug.Log($"Fake head attached: {headType}");
        }
    }
    
    public void ResetEvent()
    {
        // Reset the entire event system
        currentState = GameState.Waiting;
        isChaseActive = false;
        isEventComplete = false;
        badEndingTriggered = false;
        playerInProximity = false;
        proximityTriggered = false;
        playerChoseToHelp = false;
        choiceMade = false;
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
        
        Debug.Log("Event system reset");
    }
    
    // Door management methods
    private void FindMissingDoors()
    {
        // Find exit doors if not assigned
        if (exitDoors == null || exitDoors.Length == 0)
        {
            doorscript[] allDoors = FindObjectsOfType<doorscript>();
            List<doorscript> exitDoorList = new List<doorscript>();
            
            foreach (var door in allDoors)
            {
                if (door.name.ToLower().Contains("exit") || door.name.ToLower().Contains("bait"))
                {
                    exitDoorList.Add(door);
                }
            }
            
            if (exitDoorList.Count > 0)
            {
                exitDoors = exitDoorList.ToArray();
                Debug.Log($"Found {exitDoors.Length} exit doors automatically");
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
    
}

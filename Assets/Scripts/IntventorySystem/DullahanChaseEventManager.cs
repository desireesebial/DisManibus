using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Manages the Dullahan chase event with a repeating cycle:
/// - Chase phase: 3 minutes of active pursuit
/// - Rest phase: 1 minute of inactivity
/// - Repeats while player is in proximity
/// </summary>
public class DullahanChaseEventManager : MonoBehaviour
{
    #region Settings & Configuration

    [Header("═══ CORE CHASE SETTINGS ═══")]
    [Tooltip("Duration of chase phase in seconds (default: 180 = 3 minutes)")]
    public float chaseDuration = 180f;

    [Tooltip("Duration of rest phase in seconds (default: 60 = 1 minute)")]
    public float restDuration = 60f;

    [Tooltip("Maximum number of chase cycles (-1 = infinite)")]
    public int maxChaseCycles = -1;

    [Tooltip("Start the chase cycle automatically when player enters proximity")]
    public bool autoStart = true;

    [Header("═══ TRIGGER CONTROL SETTINGS ═══")]
    [Tooltip("Allow event to re-trigger when player leaves and re-enters proximity")]
    public bool allowRetriggerOnReenter = false;

    [Tooltip("Maximum number of times event can be triggered (-1 = unlimited, 1 = once, 3 = three times, etc.)")]
    public int maxTriggerCount = 1;

    [Tooltip("If true, event completes all cycles independently after being triggered (player can leave area)")]
    public bool runIndependentlyAfterTrigger = true;

    [Header("═══ PROXIMITY DETECTION ═══")]
    [Tooltip("Detection method: Distance uses radius check, Trigger uses OnTriggerEnter/Exit")]
    public ProximityMode proximityMode = ProximityMode.Distance;

    [Tooltip("Radius for distance-based proximity detection")]
    public float proximityRadius = 10f;

    [Tooltip("Center point for proximity (if null, uses this GameObject's position)")]
    public Transform proximityCenter;

    [Tooltip("Layer mask for player detection")]
    public LayerMask playerLayer = -1;

    [Header("═══ REQUIRED INTEGRATION ═══")]
    [Tooltip("The chase system that controls Dullahan AI behavior")]
    public DullahanChaseSystem dullahanChaseSystem;

    [Header("═══ OPTIONAL INTEGRATION ═══")]
    [Tooltip("Indicates if player chose to help Dullahan (can be set by external systems)")]
    public bool playerChoseToHelp = false;

    [Tooltip("Indicates if player made a choice (can be set by external systems)")]
    public bool choiceMade = false;

    [Header("═══ OPTIONAL FEATURES ═══")]
    [Tooltip("Enable UI display during chase and rest")]
    public bool enableUI = true;

    [Tooltip("Enable audio feedback")]
    public bool enableAudio = true;

    [Tooltip("Enable particle effects")]
    public bool enableParticles = true;

    [Tooltip("Enable door management")]
    public bool enableDoors = false;

    #endregion

    #region Optional Components

    [Header("═══ UI (Optional) ═══")]
    public GameObject chaseUI;
    public TMPro.TextMeshProUGUI chaseText;

    [Header("Timer UI (Optional)")]
    [Tooltip("Fill image for visual countdown timer")]
    public UnityEngine.UI.Image timerFillImage;

    [Tooltip("Background image for the timer bar")]
    public UnityEngine.UI.Image timerBackgroundImage;

    [Tooltip("Show progress bar (requires timerFillImage)")]
    public bool showProgressBar = true;

    [Tooltip("Show percentage remaining in text")]
    public bool showPercentage = false;

    [Header("Timer Colors")]
    [Tooltip("Color during chase phase")]
    public Color chaseColor = new Color(1f, 0.2f, 0.2f); // Red

    [Tooltip("Color during rest phase")]
    public Color restColor = new Color(0.2f, 1f, 0.2f); // Green

    [Tooltip("Color when time is running out")]
    public Color warningColor = new Color(1f, 0.6f, 0f); // Orange

    [Tooltip("Seconds remaining to trigger warning color")]
    public float warningThreshold = 30f;

    [Header("═══ AUDIO (Optional) ═══")]
    public AudioSource audioSource;
    public AudioClip chaseStartSound;
    public AudioClip chaseEndSound;
    public AudioClip restStartSound;

    [Header("═══ VISUAL EFFECTS (Optional) ═══")]
    public ParticleSystem chaseParticles;
    public ParticleSystem restParticles;

    [Header("═══ DOORS (Optional) ═══")]
    public doorscript[] exitDoors;
    public doorscript realHeadDoor;
    public int exitDoorKeyID = 999;
    public int realHeadDoorKeyID = 100;

    #endregion

    #region Unity Events

    [Header("═══ EVENTS ═══")]
    [Tooltip("Called when player enters proximity")]
    public UnityEvent OnPlayerEnterProximity;

    [Tooltip("Called when player leaves proximity")]
    public UnityEvent OnPlayerExitProximity;

    [Tooltip("Called when chase phase starts")]
    public UnityEvent OnChaseStart;

    [Tooltip("Called when chase phase ends")]
    public UnityEvent OnChaseEnd;

    [Tooltip("Called when rest phase starts")]
    public UnityEvent OnRestStart;

    [Tooltip("Called when rest phase ends")]
    public UnityEvent OnRestEnd;

    #endregion

    #region State Management

    public enum EventState
    {
        Waiting,     // Waiting for player to enter proximity
        Chasing,     // Active chase period
        Resting,     // Rest period
        Paused,      // Manually paused
        Stopped      // Event stopped/ended
    }

    public enum ProximityMode
    {
        Distance,    // Check distance from proximity center
        Trigger,     // Use OnTriggerEnter/Exit (requires Collider with IsTrigger=true)
        Both         // Use both methods
    }

    [Header("═══ DEBUG INFO (Read Only) ═══")]
    [SerializeField] private EventState currentState = EventState.Waiting;
    [SerializeField] private bool playerInProximity = false;
    [SerializeField] private bool wasPlayerInProximity = false;
    [SerializeField] private int currentCycleCount = 0;
    [SerializeField] private float stateTimer = 0f;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private int currentTriggerCount = 0;
    [SerializeField] private bool isEventRunning = false;
    [SerializeField] private bool hasBeenTriggered = false;

    // Private references
    private Transform player;
    private Coroutine eventLoopCoroutine;
    private bool isInitialized = false;

    // Public read-only properties
    public EventState CurrentState => currentState;
    public bool PlayerInProximity => playerInProximity;
    public int CurrentCycleCount => currentCycleCount;
    public float StateTimer => stateTimer;
    public bool IsPaused => isPaused;

    #endregion

    #region Initialization

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("[DullahanChase] Player found: " + player.name);
        }
        else
        {
            Debug.LogWarning("[DullahanChase] Player not found! Make sure player has 'Player' tag.");
        }

        // Auto-find chase system if not assigned
        if (dullahanChaseSystem == null)
        {
            dullahanChaseSystem = FindObjectOfType<DullahanChaseSystem>();
            if (dullahanChaseSystem == null)
                Debug.LogWarning("[DullahanChase] DullahanChaseSystem not found! Chase behavior will not work.");
        }

        // Setup audio source
        if (enableAudio && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set proximity center to self if not specified
        if (proximityCenter == null)
            proximityCenter = transform;

        // Validate trigger-based proximity
        if (proximityMode == ProximityMode.Trigger || proximityMode == ProximityMode.Both)
        {
            Collider col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
            {
                Debug.LogWarning("[DullahanChase] Trigger mode requires a Collider component with IsTrigger=true!");
            }
        }

        // Hide timer UI initially
        HideTimerUI();

        isInitialized = true;

        // Start the event loop
        if (autoStart)
        {
            StartEventLoop();
        }

        Debug.Log($"[DullahanChase] Initialized. Mode: {proximityMode}, Chase: {chaseDuration}s, Rest: {restDuration}s");
    }

    #endregion

    #region Proximity Detection

    void Update()
    {
        if (!isInitialized || isPaused || player == null) return;

        // Distance-based proximity check
        if (proximityMode == ProximityMode.Distance || proximityMode == ProximityMode.Both)
        {
            CheckDistanceProximity();
        }

        // If running independently, don't stop when player leaves
        if (!runIndependentlyAfterTrigger)
        {
            // Handle player leaving during active states (old behavior)
            if (!playerInProximity && (currentState == EventState.Chasing || currentState == EventState.Resting))
            {
                OnPlayerLeftProximity();
            }
        }
    }

    void CheckDistanceProximity()
    {
        Vector3 centerPos = proximityCenter != null ? proximityCenter.position : transform.position;
        float distance = Vector3.Distance(centerPos, player.position);

        // Update proximity state
        wasPlayerInProximity = playerInProximity;
        playerInProximity = distance <= proximityRadius;

        // Detect entering (transition from outside to inside)
        if (playerInProximity && !wasPlayerInProximity)
        {
            OnPlayerEnteredProximity();
        }
        // Detect exiting (transition from inside to outside)
        else if (!playerInProximity && wasPlayerInProximity)
        {
            OnPlayerLeftProximity();
        }
    }

    // Trigger-based proximity detection (requires Collider with IsTrigger=true)
    void OnTriggerEnter(Collider other)
    {
        if (proximityMode == ProximityMode.Trigger || proximityMode == ProximityMode.Both)
        {
            if (IsPlayer(other.gameObject))
            {
                playerInProximity = true;
                OnPlayerEnteredProximity();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (proximityMode == ProximityMode.Trigger || proximityMode == ProximityMode.Both)
        {
            if (IsPlayer(other.gameObject))
            {
                playerInProximity = false;
                OnPlayerLeftProximity();
            }
        }
    }

    bool IsPlayer(GameObject obj)
    {
        // Check by tag
        if (obj.CompareTag("Player"))
            return true;

        // Check by layer if layer mask is set
        if (playerLayer.value != -1)
        {
            return ((1 << obj.layer) & playerLayer.value) != 0;
        }

        return false;
    }

    void OnPlayerEnteredProximity()
    {
        Debug.Log("[DullahanChase] Player entered proximity!");

        // Invoke event
        OnPlayerEnterProximity?.Invoke();

        // Check if we can trigger the event
        bool canTrigger = false;

        // First time entering
        if (!hasBeenTriggered)
        {
            canTrigger = true;
            Debug.Log("[DullahanChase] First time trigger - event will start");
        }
        // Re-entering after event completed
        else if (allowRetriggerOnReenter && !isEventRunning && currentState == EventState.Waiting)
        {
            // Check if we haven't exceeded max trigger count
            if (maxTriggerCount < 0 || currentTriggerCount < maxTriggerCount)
            {
                canTrigger = true;
                Debug.Log($"[DullahanChase] Re-trigger allowed - trigger count: {currentTriggerCount + 1}/{maxTriggerCount}");
            }
            else
            {
                Debug.Log($"[DullahanChase] Max trigger count reached ({maxTriggerCount}) - event will not trigger");
            }
        }

        // Start chase cycle if allowed
        if (canTrigger && currentState == EventState.Waiting)
        {
            hasBeenTriggered = true;
            isEventRunning = true;
            currentTriggerCount++;
            currentCycleCount = 0;
            TransitionToState(EventState.Chasing);
            Debug.Log($"[DullahanChase] ✓ Event triggered! (Trigger #{currentTriggerCount})");
        }
    }

    void OnPlayerLeftProximity()
    {
        Debug.Log("[DullahanChase] Player left proximity!");

        // Invoke event
        OnPlayerExitProximity?.Invoke();

        // Only stop if not running independently
        if (!runIndependentlyAfterTrigger)
        {
            // Stop active chase/rest cycle (old behavior)
            if (currentState == EventState.Chasing || currentState == EventState.Resting)
            {
                TransitionToState(EventState.Stopped);
            }
        }
        else
        {
            Debug.Log("[DullahanChase] Player left proximity, but event continues running independently");
        }
    }

    #endregion

    #region Event Loop & State Handlers

    void StartEventLoop()
    {
        if (eventLoopCoroutine != null)
        {
            StopCoroutine(eventLoopCoroutine);
        }

        eventLoopCoroutine = StartCoroutine(EventLoop());
    }

    void StopEventLoop()
    {
        if (eventLoopCoroutine != null)
        {
            StopCoroutine(eventLoopCoroutine);
            eventLoopCoroutine = null;
        }
    }

    IEnumerator EventLoop()
    {
        while (true)
        {
            switch (currentState)
            {
                case EventState.Waiting:
                    yield return HandleWaitingState();
                    break;

                case EventState.Chasing:
                    yield return HandleChasingState();
                    break;

                case EventState.Resting:
                    yield return HandleRestingState();
                    break;

                case EventState.Paused:
                    yield return HandlePausedState();
                    break;

                case EventState.Stopped:
                    yield return HandleStoppedState();
                    break;
            }

            yield return null;
        }
    }

    IEnumerator HandleWaitingState()
    {
        Debug.Log("[DullahanChase] Waiting for player to enter proximity...");

        // Just wait until proximity is triggered
        while (currentState == EventState.Waiting)
        {
            yield return null;
        }
    }

    IEnumerator HandleChasingState()
    {
        Debug.Log($"[DullahanChase] === CHASE #{currentCycleCount + 1} STARTED === Duration: {chaseDuration}s");

        // Start chase
        StartChase();

        // Run chase timer
        stateTimer = 0f;
        while (stateTimer < chaseDuration && currentState == EventState.Chasing)
        {
            stateTimer += Time.deltaTime;

            // Update timer UI (text, progress bar, colors)
            UpdateTimerUI(stateTimer, chaseDuration, true);

            yield return null;
        }

        // End chase if we completed the duration
        if (currentState == EventState.Chasing)
        {
            EndChase();
            currentCycleCount++;

            // Check if we've reached max cycles
            if (maxChaseCycles > 0 && currentCycleCount >= maxChaseCycles)
            {
                Debug.Log("[DullahanChase] Max cycles reached. Stopping event.");
                TransitionToState(EventState.Stopped);
            }
            else
            {
                TransitionToState(EventState.Resting);
            }
        }
    }

    IEnumerator HandleRestingState()
    {
        Debug.Log($"[DullahanChase] === REST PERIOD STARTED === Duration: {restDuration}s");

        // Start rest period
        StartRest();

        // Run rest timer
        stateTimer = 0f;
        while (stateTimer < restDuration && currentState == EventState.Resting)
        {
            stateTimer += Time.deltaTime;

            // Update timer UI (text, progress bar, colors)
            UpdateTimerUI(stateTimer, restDuration, false);

            yield return null;
        }

        // End rest if we completed the duration
        if (currentState == EventState.Resting)
        {
            EndRest();

            // If running independently, always continue with next chase cycle (don't check proximity)
            if (runIndependentlyAfterTrigger)
            {
                // Check if we should continue cycling
                if (maxChaseCycles < 0 || currentCycleCount < maxChaseCycles)
                {
                    Debug.Log("[DullahanChase] Rest ended - starting next chase cycle");
                    TransitionToState(EventState.Chasing);
                }
                else
                {
                    Debug.Log($"[DullahanChase] All cycles completed ({currentCycleCount}/{maxChaseCycles})");
                    TransitionToState(EventState.Stopped);
                }
            }
            else
            {
                // Old behavior: Check if player is still in proximity
                if (playerInProximity)
                {
                    TransitionToState(EventState.Chasing);
                }
                else
                {
                    Debug.Log("[DullahanChase] Player not in proximity. Returning to waiting state.");
                    TransitionToState(EventState.Waiting);
                }
            }
        }
    }

    IEnumerator HandlePausedState()
    {
        // Just wait until unpaused
        while (currentState == EventState.Paused)
        {
            yield return null;
        }
    }

    IEnumerator HandleStoppedState()
    {
        Debug.Log("[DullahanChase] Event stopped. Cleaning up...");

        // Cleanup
        CleanupChase();
        CleanupRest();

        // Hide timer UI
        HideTimerUI();

        // Mark event as no longer running
        isEventRunning = false;

        // Wait a moment before resetting to waiting
        yield return new WaitForSeconds(2f);

        // Reset to waiting if still stopped
        if (currentState == EventState.Stopped)
        {
            currentCycleCount = 0;
            TransitionToState(EventState.Waiting);
            Debug.Log("[DullahanChase] Event ready for re-trigger (if enabled)");
        }
    }

    void TransitionToState(EventState newState)
    {
        EventState oldState = currentState;
        currentState = newState;
        stateTimer = 0f;

        Debug.Log($"[DullahanChase] State transition: {oldState} → {newState}");
    }

    #endregion

    #region Timer UI Management

    /// <summary>
    /// Updates the timer UI with countdown, progress bar, and color changes
    /// </summary>
    /// <param name="currentTime">Time elapsed since state started</param>
    /// <param name="maxTime">Total duration of the current state</param>
    /// <param name="isChase">True if in chase phase, false if in rest phase</param>
    void UpdateTimerUI(float currentTime, float maxTime, bool isChase)
    {
        if (!enableUI) return;

        float remaining = maxTime - currentTime;
        float progress = Mathf.Clamp01(remaining / maxTime);

        // Update text countdown
        if (chaseText != null)
        {
            // Format time display
            string timeDisplay;
            if (isChase)
            {
                // Chase: Show minutes and seconds
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                timeDisplay = $"{minutes}:{seconds:00}";
            }
            else
            {
                // Rest: Show seconds only
                int seconds = Mathf.FloorToInt(remaining);
                timeDisplay = $"{seconds}s";
            }

            // Build display text
            string phaseText = isChase ? "DULLAHAN CHASING! Survive:" : "REST PERIOD - Next chase in:";
            string displayText = $"{phaseText} {timeDisplay}";

            // Add percentage if enabled
            if (showPercentage)
            {
                int percentage = Mathf.FloorToInt(progress * 100f);
                displayText += $" ({percentage}%)";
            }

            chaseText.text = displayText;

            // Update text color based on time remaining
            if (remaining <= warningThreshold && isChase)
            {
                chaseText.color = warningColor;
            }
            else
            {
                chaseText.color = isChase ? chaseColor : restColor;
            }
        }

        // Update progress bar fill
        if (showProgressBar && timerFillImage != null)
        {
            timerFillImage.fillAmount = progress;

            // Update fill color based on time remaining
            if (remaining <= warningThreshold && isChase)
            {
                timerFillImage.color = warningColor;
            }
            else
            {
                timerFillImage.color = isChase ? chaseColor : restColor;
            }
        }

        // Update background color (optional, more subtle)
        if (timerBackgroundImage != null)
        {
            Color bgColor = isChase ? chaseColor : restColor;
            bgColor.a = 0.3f; // Semi-transparent background
            timerBackgroundImage.color = bgColor;
        }
    }

    /// <summary>
    /// Hides the timer UI
    /// </summary>
    void HideTimerUI()
    {
        if (chaseUI != null)
        {
            chaseUI.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the timer UI
    /// </summary>
    void ShowTimerUI()
    {
        if (enableUI && chaseUI != null)
        {
            chaseUI.SetActive(true);
        }
    }

    #endregion

    #region Chase & Rest Handlers

    void StartChase()
    {
        // Activate chase system
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.StartChase();
        }

        // Play audio
        if (enableAudio && audioSource != null && chaseStartSound != null)
        {
            audioSource.PlayOneShot(chaseStartSound);
        }

        // Start particles
        if (enableParticles && chaseParticles != null)
        {
            chaseParticles.Play();
        }

        // Show timer UI
        ShowTimerUI();

        // Invoke event
        OnChaseStart?.Invoke();
    }

    void EndChase()
    {
        // Deactivate chase system
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
        }

        // Play audio
        if (enableAudio && audioSource != null && chaseEndSound != null)
        {
            audioSource.PlayOneShot(chaseEndSound);
        }

        // Stop particles
        if (enableParticles && chaseParticles != null)
        {
            chaseParticles.Stop();
        }

        // Invoke event
        OnChaseEnd?.Invoke();

        Debug.Log("[DullahanChase] === CHASE ENDED ===");
    }

    void StartRest()
    {
        // Ensure chase is fully stopped
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
        }

        // Play audio
        if (enableAudio && audioSource != null && restStartSound != null)
        {
            audioSource.PlayOneShot(restStartSound);
        }

        // Start particles
        if (enableParticles && restParticles != null)
        {
            restParticles.Play();
        }

        // Show timer UI
        ShowTimerUI();

        // Invoke event
        OnRestStart?.Invoke();
    }

    void EndRest()
    {
        // Stop particles
        if (enableParticles && restParticles != null)
        {
            restParticles.Stop();
        }

        // Invoke event
        OnRestEnd?.Invoke();

        Debug.Log("[DullahanChase] === REST ENDED ===");
    }

    void CleanupChase()
    {
        if (dullahanChaseSystem != null)
        {
            dullahanChaseSystem.EndChase();
        }

        if (enableParticles && chaseParticles != null)
        {
            chaseParticles.Stop();
        }
    }

    void CleanupRest()
    {
        if (enableParticles && restParticles != null)
        {
            restParticles.Stop();
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually start the chase event (useful for testing or triggered events)
    /// </summary>
    public void ManualStart()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        Debug.Log("[DullahanChase] Manual start triggered");
        currentCycleCount = 0;
        playerInProximity = true; // Assume player is in proximity
        TransitionToState(EventState.Chasing);
    }

    /// <summary>
    /// Manually stop the chase event
    /// </summary>
    public void ManualStop()
    {
        Debug.Log("[DullahanChase] Manual stop triggered");
        TransitionToState(EventState.Stopped);
    }

    /// <summary>
    /// Pause the chase event (can be resumed)
    /// </summary>
    public void Pause()
    {
        if (currentState != EventState.Paused && currentState != EventState.Stopped)
        {
            isPaused = true;

            // Store previous state if we want to resume to it
            TransitionToState(EventState.Paused);

            // Pause chase system
            if (dullahanChaseSystem != null)
            {
                dullahanChaseSystem.EndChase();
            }

            Debug.Log("[DullahanChase] Paused");
        }
    }

    /// <summary>
    /// Resume from paused state
    /// </summary>
    public void Resume()
    {
        if (currentState == EventState.Paused)
        {
            isPaused = false;

            // Resume to appropriate state based on player proximity
            if (playerInProximity)
            {
                TransitionToState(EventState.Chasing);
            }
            else
            {
                TransitionToState(EventState.Waiting);
            }

            Debug.Log("[DullahanChase] Resumed");
        }
    }

    /// <summary>
    /// Reset the event to initial state
    /// </summary>
    public void Reset()
    {
        Debug.Log("[DullahanChase] Reset triggered");

        CleanupChase();
        CleanupRest();

        currentCycleCount = 0;
        stateTimer = 0f;
        playerInProximity = false;
        wasPlayerInProximity = false;
        isPaused = false;
        isEventRunning = false;
        hasBeenTriggered = false;
        currentTriggerCount = 0;

        TransitionToState(EventState.Waiting);

        Debug.Log("[DullahanChase] Event fully reset - ready for fresh trigger");
    }

    /// <summary>
    /// Reset only the trigger count (allows event to be triggered again without full reset)
    /// </summary>
    public void ResetTriggerCount()
    {
        currentTriggerCount = 0;
        hasBeenTriggered = false;
        Debug.Log("[DullahanChase] Trigger count reset - event can be triggered again");
    }

    /// <summary>
    /// Get current trigger count
    /// </summary>
    public int GetTriggerCount()
    {
        return currentTriggerCount;
    }

    /// <summary>
    /// Get maximum allowed trigger count
    /// </summary>
    public int GetMaxTriggerCount()
    {
        return maxTriggerCount;
    }

    /// <summary>
    /// Check if event is currently running
    /// </summary>
    public bool IsEventRunning()
    {
        return isEventRunning;
    }

    /// <summary>
    /// Check if event can be triggered (respects trigger count limit)
    /// </summary>
    public bool CanTrigger()
    {
        if (isEventRunning) return false;
        if (currentState != EventState.Waiting) return false;
        if (maxTriggerCount >= 0 && currentTriggerCount >= maxTriggerCount) return false;
        return true;
    }

    /// <summary>
    /// Skip current phase (for testing)
    /// </summary>
    public void SkipCurrentPhase()
    {
        Debug.Log("[DullahanChase] Skipping current phase");

        if (currentState == EventState.Chasing)
        {
            stateTimer = chaseDuration; // Force completion
        }
        else if (currentState == EventState.Resting)
        {
            stateTimer = restDuration; // Force completion
        }
    }

    #endregion

    #region Integration Callbacks

    /// <summary>
    /// Called by DullahanChaseSystem when chase starts
    /// </summary>
    public void OnChaseStarted()
    {
        Debug.Log("[DullahanChase] OnChaseStarted callback received from DullahanChaseSystem");
        // This can be used for additional integration if needed
        // The main chase logic is already handled by StartChase()
    }

    /// <summary>
    /// Called by DullahanChaseSystem when chase ends
    /// </summary>
    public void OnChaseEnded()
    {
        Debug.Log("[DullahanChase] OnChaseEnded callback received from DullahanChaseSystem");
        // This can be used for additional integration if needed
        // The main chase logic is already handled by EndChase()
    }

    #endregion

    #region Optional Door Management

    void OpenExitDoors()
    {
        if (!enableDoors || exitDoors == null) return;

        foreach (doorscript door in exitDoors)
        {
            if (door != null)
            {
                door.SetRequiredKeyID(exitDoorKeyID);
                door.UnlockDoor();
                door.OpenDoor();
            }
        }
    }

    void OpenRealHeadDoor()
    {
        if (!enableDoors || realHeadDoor == null) return;

        realHeadDoor.SetRequiredKeyID(realHeadDoorKeyID);
        realHeadDoor.UnlockDoor();
        realHeadDoor.OpenDoor();
    }

    #endregion

    #region Debug & Visualization

    void OnDrawGizmosSelected()
    {
        // Draw proximity radius (only for distance-based mode)
        if (proximityMode == ProximityMode.Distance || proximityMode == ProximityMode.Both)
        {
            Vector3 centerPos = proximityCenter != null ? proximityCenter.position : transform.position;

            Gizmos.color = playerInProximity ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(centerPos, proximityRadius);

            // Draw line to proximity center if it's different from this object
            if (proximityCenter != null && proximityCenter != transform)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, proximityCenter.position);
            }
        }

        // Draw state info in editor
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            string label = $"State: {currentState}\n" +
                          $"Cycle: {currentCycleCount + 1}\n" +
                          $"Timer: {stateTimer:F1}s\n" +
                          $"In Proximity: {playerInProximity}";

            UnityEditor.Handles.Label(labelPos, label);
        }
        #endif
    }

    // Debug method to print current status
    [ContextMenu("Print Status")]
    void PrintStatus()
    {
        Debug.Log("=== DULLAHAN CHASE EVENT STATUS ===");
        Debug.Log($"State: {currentState}");
        Debug.Log($"Cycle: {currentCycleCount + 1}/{(maxChaseCycles < 0 ? "∞" : maxChaseCycles.ToString())}");
        Debug.Log($"Timer: {stateTimer:F1}s");
        Debug.Log($"Player In Proximity: {playerInProximity}");
        Debug.Log($"Paused: {isPaused}");
        Debug.Log($"Proximity Mode: {proximityMode}");
        Debug.Log("--- TRIGGER INFO ---");
        Debug.Log($"Event Running: {isEventRunning}");
        Debug.Log($"Has Been Triggered: {hasBeenTriggered}");
        Debug.Log($"Trigger Count: {currentTriggerCount}/{(maxTriggerCount < 0 ? "∞" : maxTriggerCount.ToString())}");
        Debug.Log($"Allow Retrigger: {allowRetriggerOnReenter}");
        Debug.Log($"Run Independently: {runIndependentlyAfterTrigger}");
        Debug.Log($"Can Trigger: {CanTrigger()}");
        Debug.Log("===================================");
    }

    #endregion

    #region Ending Choice Management (Static Methods)

    /// <summary>
    /// Static method to save the player's ending choice (persists across scenes)
    /// </summary>
    /// <param name="choseGoodEnding">True for good ending, false for bad ending</param>
    public static void SaveEndingChoice(bool choseGoodEnding)
    {
        PlayerPrefs.SetInt("Floor2EndingChoice", choseGoodEnding ? 1 : 0);
        PlayerPrefs.SetString("Floor2EndingChoiceTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();

        Debug.Log($"[DullahanChase] Ending choice saved: {(choseGoodEnding ? "Good Ending" : "Bad Ending")}");
    }

    /// <summary>
    /// Static method to get the player's ending choice (persists across scenes)
    /// </summary>
    /// <returns>True if good ending was chosen, false otherwise</returns>
    public static bool GetEndingChoice()
    {
        return PlayerPrefs.GetInt("Floor2EndingChoice", 0) == 1;
    }

    /// <summary>
    /// Static method to clear the ending choice (useful for new game)
    /// </summary>
    public static void ClearEndingChoice()
    {
        PlayerPrefs.DeleteKey("Floor2EndingChoice");
        PlayerPrefs.DeleteKey("Floor2EndingChoiceTime");
        PlayerPrefs.Save();

        Debug.Log("[DullahanChase] Ending choice cleared");
    }

    #endregion
}

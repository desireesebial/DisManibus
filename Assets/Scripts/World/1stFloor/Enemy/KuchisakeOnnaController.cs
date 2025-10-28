using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the Kuchisake-onna enemy AI behavior including patrol, detection, questioning, and chase mechanics.
/// </summary>
public class KuchisakeOnnaController : MonoBehaviour
{
    #region Constants
    private const float PATROL_POINT_REACHED_DISTANCE = 0.5f;
    private const float CATCH_PLAYER_DISTANCE = 2f;
    private const float AGENT_MOVING_THRESHOLD = 0.1f;
    private const float RAYCAST_EYE_HEIGHT = 1.5f;
    private const float CHASE_UPDATE_DISTANCE_THRESHOLD = 1f;
    private const float FIRST_QUESTION_DELAY = 1f;
    private const float SMOOTH_LOOK_DURATION = 1.5f;
    private const float YES_SEQUENCE_MASK_DELAY = 1f;
    private const float YES_SEQUENCE_PAUSE = 2f;
    private const float NO_SEQUENCE_DELAY = 1f;
    private const float NO_SEQUENCE_STANDUP_MULTIPLIER = 0.7f;
    private const float MAYBE_SEQUENCE_DELAY = 1.5f;
    private const float MAYBE_SEQUENCE_PAUSE = 1.5f;

    // Animation distance states (for distanceFromPlayer parameter)
    private const int ANIM_DISTANCE_IDLE = 0;      // Sitting/Idle state
    private const int ANIM_DISTANCE_WALKING = 1;   // Walking/Patrol state
    private const int ANIM_DISTANCE_RUNNING = 2;   // Running/Chase state
    #endregion

    #region Serialized Fields
    [Header("AI Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float chaseTimeout = 30f;
    [SerializeField] private float escapeDistance = 20f;

    [Header("Question Settings")]
    [SerializeField] private float questionTimer = 5f;
    [SerializeField] private float chaseChanceOnYes = 0.7f; // 70% chance she chases even on "Yes"
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scissorsSnipSound;
    [SerializeField] private AudioClip questionVoiceClip;
    [SerializeField] private AudioClip deathScreamClip;
    [SerializeField] private AudioClip angerSound;
    [SerializeField] private AudioClip retreatLaughSound;
    [SerializeField] private float ambientScissorInterval = 5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject maskObject; // Mask on face
    [SerializeField] private Material normalFaceMaterial;
    [SerializeField] private Material slitMouthMaterial;
    [SerializeField] private Renderer faceRenderer;

    [Header("First Encounter")]
    [SerializeField] private bool requireFirstEncounter = true;
    [SerializeField] private Transform firstEncounterPosition; // Where she sits initially
    [SerializeField] private float firstEncounterTriggerDistance = 5f; // Distance to trigger first encounter
    [SerializeField] private Animator animator; // For sitting/standing animations
    [SerializeField] private string nearPlayerTrigger = "nearPlayer"; // Trigger when player is near (for standing up)
    [SerializeField] private float standUpDuration = 2f; // Time for stand up animation

    [Header("Movement Animations")]
    [SerializeField] private string distanceParameter = "distanceFromPlayer"; // Int parameter for distance-based animations
    [Tooltip("Optional: Float parameter for walking speed control (0=idle, 1=walk)")]
    [SerializeField] private string speedParameter = "Speed"; // Optional speed parameter
    
    [Header("References")]
    [SerializeField] private KuchisakeQuestionUI questionUI;
    [SerializeField] private Transform player;

    [Header("Video Trigger Integration")]
    [Tooltip("Reference to the VideoTrigger that activates persistent chase mode")]
    [SerializeField] private VideoTrigger videoTrigger;
    #endregion

    #region Private Fields
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float chaseTimer;
    private float ambientScissorTimer;
    private bool isActive = true;
    private bool hasHadFirstEncounter = false;
    private bool isStandingUp = false;

    // Chase optimization
    private Vector3 lastChaseTargetPosition;

    // Persistent chase mode (activated by video trigger)
    private bool isPersistentChase = false;
    private bool isPlayerInSafeZone = false;
    private bool hasMonitoredVideoTrigger = false; // Prevents multiple activations
    private Coroutine maskRestoreCoroutine = null; // Tracks delayed mask restoration

    // Coroutine tracking
    private List<Coroutine> runningCoroutines = new List<Coroutine>();
    #endregion

    #region Enums

    public enum EnemyState
    {
        WaitingFirstEncounter, // Sitting, waiting for player
        FirstEncounter,        // First question encounter
        StandingUp,           // Animation of standing up
        Patrol,
        Question,
        Chase,
        Retreat,
        Disabled
    }

    public enum AnimationState
    {
        Idle,
        Walking,
        Running
    }
    #endregion

    #region Private State
    private EnemyState currentState = EnemyState.WaitingFirstEncounter;
    private AnimationState currentAnimationState = AnimationState.Idle;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initializes the enemy controller and sets up initial state.
    /// </summary>
    void Start()
    {
        // Validate and initialize NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[KuchisakeOnna] {gameObject.name}: NavMeshAgent component not found! Enemy will not function.");
            enabled = false;
            return;
        }
        agent.speed = patrolSpeed;

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: Player not found! Enemy will not detect player.");
            }
        }

        // Validate required references
        ValidateSetup();

        // Initialize timers
        ambientScissorTimer = ambientScissorInterval;
        lastChaseTargetPosition = Vector3.zero;

        // Setup first encounter behavior
        if (requireFirstEncounter)
        {
            SetupFirstEncounter();
        }
        else
        {
            // Skip first encounter, start patrolling immediately
            hasHadFirstEncounter = true;
            currentState = EnemyState.Patrol;

            if (HasValidPatrolPoints())
            {
                GoToNextPatrolPoint();
            }
        }
    }

    /// <summary>
    /// Update is called once per frame. Handles state machine and ambient effects.
    /// </summary>
    void Update()
    {
        if (!isActive || player == null) return;

        // Monitor video trigger for persistent chase activation
        MonitorVideoTrigger();

        // Ambient scissor sounds (only after first encounter)
        if (hasHadFirstEncounter)
        {
            ambientScissorTimer -= Time.deltaTime;
            if (ambientScissorTimer <= 0)
            {
                PlayAmbientScissors();
                ambientScissorTimer = ambientScissorInterval;
            }
        }

        // State machine
        switch (currentState)
        {
            case EnemyState.WaitingFirstEncounter:
                HandleWaitingForFirstEncounter();
                break;
            case EnemyState.FirstEncounter:
                HandleQuestion();
                break;
            case EnemyState.StandingUp:
                // Wait for stand up animation to complete
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Question:
                HandleQuestion();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Retreat:
                HandleRetreat();
                break;
        }
    }

    /// <summary>
    /// Cleanup running coroutines when disabled.
    /// </summary>
    void OnDisable()
    {
        StopAllTrackedCoroutines();
    }

    /// <summary>
    /// Cleanup running coroutines when destroyed.
    /// </summary>
    void OnDestroy()
    {
        StopAllTrackedCoroutines();
    }
    #endregion

    #region Initialization & Validation
    /// <summary>
    /// Validates that all required components and references are properly set up.
    /// </summary>
    private void ValidateSetup()
    {
        if (audioSource == null)
        {
            Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: AudioSource not assigned. Enemy will have no sound.");
        }

        if (questionUI == null)
        {
            Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: KuchisakeQuestionUI not assigned. Question mechanic will not work.");
        }

        if (animator != null)
        {
            // Validate distance parameter exists
            if (!string.IsNullOrEmpty(distanceParameter))
            {
                if (!HasAnimationParameter(distanceParameter))
                {
                    Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: Animation parameter '{distanceParameter}' not found in Animator.");
                }
            }
            else
            {
                Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: Distance parameter is empty! Animation system will not work.");
            }

            // Validate nearPlayer trigger exists
            if (!string.IsNullOrEmpty(nearPlayerTrigger) && !HasAnimationParameter(nearPlayerTrigger))
            {
                Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: Animation parameter '{nearPlayerTrigger}' not found in Animator.");
            }

            // Check optional speed parameter
            if (!string.IsNullOrEmpty(speedParameter) && !HasAnimationParameter(speedParameter))
            {
                Debug.Log($"[KuchisakeOnna] {gameObject.name}: Optional speed parameter '{speedParameter}' not found. This is OK if not using speed control.");
            }
        }
        else
        {
            Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: Animator component not assigned! Animations will not play.");
        }

        if (!HasValidPatrolPoints())
        {
            Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: No valid patrol points assigned. Enemy cannot patrol.");
        }
    }

    /// <summary>
    /// Checks if patrol points array contains at least one valid patrol point.
    /// </summary>
    private bool HasValidPatrolPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return false;

        foreach (var point in patrolPoints)
        {
            if (point != null)
                return true;
        }
        return false;
    }

    void SetupFirstEncounter()
    {
        hasHadFirstEncounter = false;
        currentState = EnemyState.WaitingFirstEncounter;
        
        // Position at first encounter location if set
        if (firstEncounterPosition != null)
        {
            transform.position = firstEncounterPosition.position;
            transform.rotation = firstEncounterPosition.rotation;
        }
        
        // Disable NavMesh agent during first encounter
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Set animation to sitting/idle state
        if (animator != null && !string.IsNullOrEmpty(distanceParameter))
        {
            animator.SetInteger(distanceParameter, ANIM_DISTANCE_IDLE);
        }
    }
    #endregion

    #region State Handlers
    /// <summary>
    /// Handles the waiting state for the first encounter with the player.
    /// </summary>
    void HandleWaitingForFirstEncounter()
    {
        // Check if player is close enough to trigger first encounter
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= firstEncounterTriggerDistance)
        {
            StartFirstEncounter();
        }
    }

    /// <summary>
    /// Handles the patrol state behavior including movement and player detection.
    /// In persistent chase mode, monitors for player leaving safe zone and immediately resumes chase.
    /// </summary>
    void HandlePatrol()
    {
        // Validate agent is enabled and functional
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // Set walking animation if moving
        if (agent.velocity.magnitude > AGENT_MOVING_THRESHOLD)
        {
            ChangeAnimationState(AnimationState.Walking);
        }
        else
        {
            ChangeAnimationState(AnimationState.Idle);
        }

        // Check if reached patrol point
        if (!agent.pathPending && agent.remainingDistance < PATROL_POINT_REACHED_DISTANCE)
        {
            GoToNextPatrolPoint();
        }

        // Check for player detection
        if (player == null)
            return;

        // PERSISTENT CHASE MODE: Check if player left safe zone
        if (isPersistentChase)
        {
            CheckPlayerSafeZone();

            if (!isPlayerInSafeZone)
            {
                // Player left safe zone - immediately resume chase (no questions!)
                Debug.Log($"[KuchisakeOnna] {gameObject.name}: Player detected outside safe zone! Resuming chase!");
                StartChase();
                return;
            }

            // Continue patrolling while player is in safe zone
            return;
        }

        // NORMAL MODE: Check for player detection with line of sight
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            // Check line of sight
            RaycastHit hit;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Vector3 rayStart = transform.position + Vector3.up * RAYCAST_EYE_HEIGHT;

            if (Physics.Raycast(rayStart, directionToPlayer, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    StartChase();
                }
            }
        }
    }

    /// <summary>
    /// Handles the question state. Question timing is managed by KuchisakeQuestionUI.
    /// </summary>
    void HandleQuestion()
    {
        // Set idle animation while asking question
        ChangeAnimationState(AnimationState.Idle);

        // Question UI handles the timer and player input
        // This is managed by the UI script
    }

    /// <summary>
    /// Handles the chase state, pursuing the player until caught or escaped.
    /// In persistent chase mode, never gives up - patrols map when player is in safe zone.
    /// </summary>
    void HandleChase()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || player == null)
            return;

        // Set running animation during chase
        ChangeAnimationState(AnimationState.Running);

        // PERSISTENT CHASE: Check if player is in safe zone (unwalkable area)
        if (isPersistentChase)
        {
            CheckPlayerSafeZone();

            if (isPlayerInSafeZone)
            {
                // Player is hiding in safe zone - patrol the entire map to find them
                Debug.Log($"[KuchisakeOnna] {gameObject.name}: Player in safe zone. Patrolling map...");
                currentState = EnemyState.Patrol;
                agent.speed = patrolSpeed;

                // Start 3-second timer to restore mask while patrolling
                if (maskRestoreCoroutine != null)
                {
                    StopCoroutine(maskRestoreCoroutine);
                }
                maskRestoreCoroutine = StartCoroutine(DelayedMaskRestore(3f));

                // Start patrolling all waypoints
                if (HasValidPatrolPoints())
                {
                    GoToNextPatrolPoint();
                }
                return;
            }
        }

        // Optimize: Only update destination if player moved significantly
        float distanceFromLastUpdate = Vector3.Distance(player.position, lastChaseTargetPosition);
        if (distanceFromLastUpdate > CHASE_UPDATE_DISTANCE_THRESHOLD)
        {
            agent.SetDestination(player.position);
            lastChaseTargetPosition = player.position;
        }

        chaseTimer -= Time.deltaTime;

        // Check if caught player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < CATCH_PLAYER_DISTANCE)
        {
            CatchPlayer();
            return;
        }

        // Check if player escaped (ONLY if NOT persistent chase mode)
        if (!isPersistentChase)
        {
            if (distanceToPlayer > escapeDistance || chaseTimer <= 0)
            {
                currentState = EnemyState.Retreat;
            }
        }
        // In persistent chase mode, NEVER retreat - chase forever
    }

    /// <summary>
    /// Handles the retreat state, teleporting to a random patrol point and resuming patrol.
    /// </summary>
    void HandleRetreat()
    {
        // Set idle animation during retreat teleport
        ChangeAnimationState(AnimationState.Idle);

        // Restore mask when retreating
        RestoreMask();

        // Teleport to random valid patrol point
        if (HasValidPatrolPoints() && agent != null)
        {
            // Find a valid patrol point
            Transform targetPoint = GetRandomValidPatrolPoint();
            if (targetPoint != null)
            {
                // Ensure agent is enabled before warping
                if (!agent.enabled)
                {
                    agent.enabled = true;
                }

                // Use Warp instead of direct position assignment for NavMeshAgent safety
                agent.Warp(targetPoint.position);

                // Play retreat sound
                PlaySound(retreatLaughSound);
            }
        }

        // Resume patrol
        currentState = EnemyState.Patrol;
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;
        }
        // Note: Removed GoToNextPatrolPoint() - HandlePatrol will navigate when she reaches the warped point
    }

    /// <summary>
    /// Moves to the next patrol point in the patrol route.
    /// </summary>
    void GoToNextPatrolPoint()
    {
        if (!HasValidPatrolPoints() || agent == null || !agent.enabled)
            return;

        // Find next valid patrol point
        int attempts = 0;
        int maxAttempts = patrolPoints.Length;

        while (attempts < maxAttempts)
        {
            if (patrolPoints[currentPatrolIndex] != null)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                return;
            }

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            attempts++;
        }

        Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: No valid patrol points found!");
    }

    /// <summary>
    /// Starts the first encounter sequence with the player.
    /// </summary>
    void StartFirstEncounter()
    {
        currentState = EnemyState.FirstEncounter;

        // Trigger nearPlayer to start stand-up animation
        if (animator != null && !string.IsNullOrEmpty(nearPlayerTrigger))
        {
            animator.SetTrigger(nearPlayerTrigger);
        }

        // Slowly face the player (no sudden movements)
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            StartTrackedCoroutine(SmoothLookAt(direction, SMOOTH_LOOK_DURATION));
        }

        // Show first question (proper first encounter sequence)
        StartTrackedCoroutine(DelayedFirstQuestion());

        // TESTING MODE: Uncomment below to skip question and go directly to standup and patrol
        // StartTrackedCoroutine(TestStandupSequence());
    }

    /// <summary>
    /// Smoothly rotates to look at a direction over time.
    /// </summary>
    IEnumerator SmoothLookAt(Vector3 direction, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    /// <summary>
    /// Plays the first question with a delay for tension.
    /// </summary>
    IEnumerator DelayedFirstQuestion()
    {
        // Wait a moment for tension
        yield return new WaitForSeconds(FIRST_QUESTION_DELAY);

        // Play question voice
        PlaySound(questionVoiceClip);

        // Show question UI
        if (questionUI != null)
        {
            questionUI.ShowQuestion(questionTimer, this);
        }
    }

    void StartQuestionSequence()
    {
        currentState = EnemyState.Question;

        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Play question voice
        if (audioSource != null && questionVoiceClip != null)
        {
            audioSource.PlayOneShot(questionVoiceClip);
        }

        // Show question UI
        if (questionUI != null)
        {
            questionUI.ShowQuestion(questionTimer, this);
        }
    }

    public void OnPlayerAnswered(KuchisakeQuestionUI.Answer answer)
    {
        // PERSISTENT CHASE MODE: Skip all question logic, just chase
        if (isPersistentChase)
        {
            Debug.Log($"[KuchisakeOnna] {gameObject.name}: Persistent chase active - ignoring question answer!");
            StartChase();
            return;
        }

        // Check if this is the first encounter
        if (!hasHadFirstEncounter)
        {
            HandleFirstEncounterAnswer(answer);
            return;
        }

        // Normal encounter behavior
        switch (answer)
        {
            case KuchisakeQuestionUI.Answer.Yes:
                // 70% chance she still chases, 30% she lets you go
                if (Random.value < chaseChanceOnYes)
                {
                    StartTrackedCoroutine(DelayedChase(1.5f));
                }
                else
                {
                    // She's pleased - retreat
                    if (agent != null)
                    {
                        agent.isStopped = false;
                    }
                    currentState = EnemyState.Retreat;
                }
                break;

            case KuchisakeQuestionUI.Answer.No:
                // Immediate anger and chase
                PlaySound(angerSound);
                StartChase();
                break;

            case KuchisakeQuestionUI.Answer.Maybe:
                // Confusion - delayed chase
                StartTrackedCoroutine(DelayedChase(2f));
                break;
        }
    }

    void HandleFirstEncounterAnswer(KuchisakeQuestionUI.Answer answer)
    {
        hasHadFirstEncounter = true;

        switch (answer)
        {
            case KuchisakeQuestionUI.Answer.Yes:
                // She seems pleased but reveals her true face
                StartTrackedCoroutine(FirstEncounterYesSequence());
                break;

            case KuchisakeQuestionUI.Answer.No:
                // She's offended - removes mask and stands
                StartTrackedCoroutine(FirstEncounterNoSequence());
                break;

            case KuchisakeQuestionUI.Answer.Maybe:
                // Confused but intrigued - stands up slowly
                StartTrackedCoroutine(FirstEncounterMaybeSequence());
                break;
        }
    }

    IEnumerator FirstEncounterYesSequence()
    {
        // Brief pause for player reaction
        yield return new WaitForSeconds(YES_SEQUENCE_MASK_DELAY);

        // Brief pause for player to see
        yield return new WaitForSeconds(YES_SEQUENCE_PAUSE);

        // Stand up animation
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);

        // Activate patrol behavior (mask stays on)
        ActivatePatrolMode(restoreMask: true);
    }

    IEnumerator FirstEncounterNoSequence()
    {
        PlaySound(angerSound);

        yield return new WaitForSeconds(NO_SEQUENCE_DELAY);

        // Stand up faster
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration * NO_SEQUENCE_STANDUP_MULTIPLIER);

        // Immediately start chasing (mask will be removed when chase starts)
        ActivatePatrolMode(restoreMask: true);
        StartChase();
    }

    IEnumerator FirstEncounterMaybeSequence()
    {
        // Tilts head (if animation available)
        yield return new WaitForSeconds(MAYBE_SEQUENCE_DELAY);

        yield return new WaitForSeconds(MAYBE_SEQUENCE_PAUSE);

        // Stand up slowly
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);

        // Activate patrol (mask stays on)
        ActivatePatrolMode(restoreMask: true);
    }

    IEnumerator TestStandupSequence()
    {
        // Wait a moment after player approaches
        yield return new WaitForSeconds(1f);

        // Stand up
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);

        // Start patrolling (restore mask for testing - or set to false for scary mode)
        ActivatePatrolMode(restoreMask: true);
    }

    void BeginStandUp()
    {
        currentState = EnemyState.StandingUp;
        isStandingUp = true;

        // Note: Stand-up animation was already triggered by nearPlayer in StartFirstEncounter()
        // This method just sets the state. Animation system handles the rest via distanceFromPlayer parameter.
    }

    void ActivatePatrolMode(bool restoreMask = true)
    {
        isStandingUp = false;
        currentState = EnemyState.Patrol;

        // Restore mask when returning to patrol/roaming (if specified)
        if (restoreMask)
        {
            RestoreMask();
        }

        // Enable NavMesh agent and warp to first patrol point
        if (agent != null && HasValidPatrolPoints())
        {
            // Warp to first patrol point before enabling agent
            // This ensures we're on NavMesh when agent is enabled
            // Validate that patrol point 0 exists (HasValidPatrolPoints only checks ANY point is valid)
            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            {
                Transform firstPoint = patrolPoints[0];
                agent.enabled = true; // Must enable before Warp

                // Check if we're on NavMesh, if not, warp to first patrol point
                if (!agent.isOnNavMesh)
                {
                    agent.Warp(firstPoint.position);
                }

                agent.speed = patrolSpeed;
                GoToNextPatrolPoint();
            }
            else
            {
                // First patrol point is null, just enable agent and use GoToNextPatrolPoint
                agent.enabled = true;
                agent.speed = patrolSpeed;
                GoToNextPatrolPoint(); // Will find first valid patrol point
            }
        }
        else if (agent != null)
        {
            // No patrol points, just enable agent
            agent.enabled = true;
            agent.speed = patrolSpeed;
        }
    }

    public void OnQuestionTimeout()
    {
        // Check if this is first encounter
        if (!hasHadFirstEncounter)
        {
            // First encounter timeout - she stands and becomes active threat
            hasHadFirstEncounter = true;

            PlaySound(angerSound);

            StartTrackedCoroutine(TimeoutStandAndActivate());
        }
        else
        {
            // Normal encounter timeout - instant death
            KillPlayer();
        }
    }

    IEnumerator TimeoutStandAndActivate()
    {
        // Stand up menacingly
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);

        // Start patrolling (mask stays on)
        ActivatePatrolMode(restoreMask: true);

        // Player escaped this time, but she's now active
    }

    void RemoveMask()
    {
        if (maskObject != null)
        {
            maskObject.SetActive(false);
        }

        if (faceRenderer != null && slitMouthMaterial != null)
        {
            faceRenderer.material = slitMouthMaterial;
        }
    }

    /// <summary>
    /// Coroutine that restores the mask after a delay.
    /// Used during persistent chase when enemy switches to patrol mode.
    /// </summary>
    IEnumerator DelayedMaskRestore(float delay)
    {
        Debug.Log($"[KuchisakeOnna] {gameObject.name}: Will restore mask in {delay} seconds...");
        yield return new WaitForSeconds(delay);

        Debug.Log($"[KuchisakeOnna] {gameObject.name}: Restoring mask (patrolling mode)");
        RestoreMask();

        maskRestoreCoroutine = null;
    }

    IEnumerator DelayedChase(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartChase();
    }

    void StartChase()
    {
        currentState = EnemyState.Chase;
        chaseTimer = chaseTimeout;

        // Cancel any pending mask restore timer
        if (maskRestoreCoroutine != null)
        {
            StopCoroutine(maskRestoreCoroutine);
            maskRestoreCoroutine = null;
        }

        // Remove mask immediately when starting chase to show slit-mouth
        RemoveMask();

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }
    }

    void CatchPlayer()
    {
        KillPlayer();
    }

    /// <summary>
    /// Kills the player when caught.
    /// </summary>
    void KillPlayer()
    {
        // Play death sounds and effects
        PlaySound(deathScreamClip);
        PlaySound(scissorsSnipSound);

        // Trigger player death
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
            else
            {
                Debug.LogWarning($"[KuchisakeOnna] {gameObject.name}: PlayerHealth component not found on player!");
            }
        }

        // Disable enemy temporarily
        currentState = EnemyState.Disabled;
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    /// <summary>
    /// Plays ambient scissor sounds when player is nearby.
    /// </summary>
    void PlayAmbientScissors()
    {
        // Only play if player is somewhat close
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < detectionRange * 2)
            {
                PlaySound(scissorsSnipSound, 0.5f);
            }
        }
    }

    /// <summary>
    /// Resets the enemy to initial state for respawn or restart.
    /// </summary>
    public void ResetEnemy()
    {
        // Stop all running coroutines
        StopAllTrackedCoroutines();

        // Cancel mask restore coroutine if running
        if (maskRestoreCoroutine != null)
        {
            StopCoroutine(maskRestoreCoroutine);
            maskRestoreCoroutine = null;
        }

        // Reset state
        currentState = requireFirstEncounter ? EnemyState.WaitingFirstEncounter : EnemyState.Patrol;
        currentAnimationState = AnimationState.Idle;
        isActive = true;
        hasHadFirstEncounter = false; // FIX: Reset first encounter flag
        isPersistentChase = false; // Reset persistent chase flag
        isPlayerInSafeZone = false; // Reset safe zone flag
        hasMonitoredVideoTrigger = false; // Reset video trigger monitoring

        // Restore mask
        RestoreMask();

        // Reset NavMeshAgent
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }

        // Reset animation to idle
        ChangeAnimationState(AnimationState.Idle);

        // Start appropriate behavior
        if (requireFirstEncounter)
        {
            SetupFirstEncounter();
        }
        else if (HasValidPatrolPoints())
        {
            GoToNextPatrolPoint();
        }
    }

    /// <summary>
    /// Monitors the VideoTrigger to detect when video has played/finished.
    /// When detected, activates persistent chase mode.
    /// </summary>
    private void MonitorVideoTrigger()
    {
        // Check if we have a video trigger to monitor
        if (videoTrigger == null)
            return;

        // Check if already monitored and activated
        if (hasMonitoredVideoTrigger)
            return;

        // Check if video has played (either finished or skipped)
        if (videoTrigger.hasPlayed)
        {
            Debug.Log($"[KuchisakeOnna] {gameObject.name}: Video trigger detected! Video has played.");

            // Mark as monitored to prevent multiple activations
            hasMonitoredVideoTrigger = true;

            // Activate persistent chase mode
            ActivatePersistentChase();
        }
    }

    /// <summary>
    /// Activates persistent chase mode. Called when video trigger has played.
    /// In this mode, the enemy will chase the player indefinitely without giving up.
    /// When player is in safe zone (unwalkable area), enemy patrols the entire map.
    /// </summary>
    public void ActivatePersistentChase()
    {
        Debug.Log($"[KuchisakeOnna] {gameObject.name}: PERSISTENT CHASE ACTIVATED! There is no escape...");

        isPersistentChase = true;

        // Skip first encounter if not done yet
        if (!hasHadFirstEncounter)
        {
            hasHadFirstEncounter = true;

            // Enable NavMesh agent
            if (agent != null)
            {
                agent.enabled = true;
            }
        }

        // Remove mask to show slit-mouth (she's revealed herself)
        RemoveMask();

        // Force immediate chase mode
        StartChase();
    }

    // Animation Methods
    void ChangeAnimationState(AnimationState newAnimationState)
    {
        if (currentAnimationState == newAnimationState)
            return;

        currentAnimationState = newAnimationState;
        UpdateAnimationParameters();
    }

    void UpdateAnimationParameters()
    {
        if (animator == null || string.IsNullOrEmpty(distanceParameter))
            return;

        // Update distance parameter based on current animation state
        int distanceValue = ANIM_DISTANCE_IDLE;
        float speedValue = 0f;

        switch (currentAnimationState)
        {
            case AnimationState.Idle:
                // Sitting/standing still - distance = 0
                distanceValue = ANIM_DISTANCE_IDLE;
                speedValue = 0f; // Frozen animation (if speed parameter exists)
                break;

            case AnimationState.Walking:
                // Walking/patrolling - distance = 1
                distanceValue = ANIM_DISTANCE_WALKING;
                speedValue = 1f; // Normal walking speed
                break;

            case AnimationState.Running:
                // Running/chasing - distance = 2
                distanceValue = ANIM_DISTANCE_RUNNING;
                speedValue = 0f; // Not used for running
                break;
        }

        // Set distance parameter (primary control)
        SetAnimInt(distanceParameter, distanceValue);

        // Set optional speed parameter for smooth idle/walk transitions
        if (!string.IsNullOrEmpty(speedParameter) && HasAnimationParameter(speedParameter))
        {
            SetAnimFloat(speedParameter, speedValue);
        }
    }

    void SetAnimInt(string paramName, int value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName) && HasAnimationParameter(paramName))
        {
            animator.SetInteger(paramName, value);
        }
    }

    void SetAnimFloat(string paramName, float value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName) && HasAnimationParameter(paramName))
        {
            animator.SetFloat(paramName, value);
        }
    }

    void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName) && HasAnimationParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    bool HasAnimationParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName))
            return false;

        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Checks if the player is in a safe zone (unwalkable area off NavMesh).
    /// Updates isPlayerInSafeZone flag.
    /// </summary>
    private void CheckPlayerSafeZone()
    {
        if (player == null)
            return;

        // Check if player position is on NavMesh
        NavMeshHit hit;
        float searchRadius = 1.5f; // Search within 1.5m of player position

        // SamplePosition returns true if a valid NavMesh position is found near the player
        bool isOnNavMesh = NavMesh.SamplePosition(player.position, out hit, searchRadius, NavMesh.AllAreas);

        // If player is NOT on NavMesh, they're in a safe zone
        bool wasInSafeZone = isPlayerInSafeZone;
        isPlayerInSafeZone = !isOnNavMesh;

        // Log when player enters/exits safe zone
        if (isPlayerInSafeZone && !wasInSafeZone)
        {
            Debug.Log($"[KuchisakeOnna] {gameObject.name}: Player entered safe zone!");
        }
        else if (!isPlayerInSafeZone && wasInSafeZone)
        {
            Debug.Log($"[KuchisakeOnna] {gameObject.name}: Player left safe zone! Resuming chase!");
        }
    }

    /// <summary>
    /// Plays a sound clip safely with null checks.
    /// </summary>
    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// Restores the mask to the enemy's face.
    /// </summary>
    private void RestoreMask()
    {
        if (maskObject != null)
        {
            maskObject.SetActive(true);
        }

        if (faceRenderer != null && normalFaceMaterial != null)
        {
            faceRenderer.material = normalFaceMaterial;
        }
    }

    /// <summary>
    /// Gets a random valid patrol point from the patrol points array.
    /// </summary>
    private Transform GetRandomValidPatrolPoint()
    {
        if (!HasValidPatrolPoints())
            return null;

        // Create list of valid patrol points
        List<Transform> validPoints = new List<Transform>();
        foreach (var point in patrolPoints)
        {
            if (point != null)
            {
                validPoints.Add(point);
            }
        }

        if (validPoints.Count == 0)
            return null;

        // Return random valid point
        int randomIndex = Random.Range(0, validPoints.Count);
        return validPoints[randomIndex];
    }

    /// <summary>
    /// Starts a coroutine and tracks it for cleanup.
    /// </summary>
    private void StartTrackedCoroutine(IEnumerator coroutine)
    {
        Coroutine c = StartCoroutine(coroutine);
        if (c != null)
        {
            runningCoroutines.Add(c);
        }
    }

    /// <summary>
    /// Stops all tracked coroutines and clears the list.
    /// </summary>
    private void StopAllTrackedCoroutines()
    {
        foreach (var coroutine in runningCoroutines)
        {
            if (coroutine != null)
            {
                try
                {
                    StopCoroutine(coroutine);
                }
                catch
                {
                    // Coroutine might already be stopped, ignore error
                }
            }
        }
        runningCoroutines.Clear();
    }
    #endregion

    #region Gizmos
    /// <summary>
    /// Draws debug visualization in the editor for ranges and patrol paths.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw first encounter trigger range
        if (requireFirstEncounter && firstEncounterPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firstEncounterPosition.position, firstEncounterTriggerDistance);
            
            // Draw line from first encounter to first patrol point
            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(firstEncounterPosition.position, patrolPoints[0].position);
            }
        }

        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw escape distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, escapeDistance);

        // Draw patrol points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    
                    // Draw line to next patrol point
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
    #endregion
}

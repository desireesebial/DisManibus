using UnityEngine;
using UnityEngine.AI;
using DisManibus.World.FirstFloor.Enemy;

/// <summary>
/// Simplified Kuchisake-onna controller with video-triggered chase behavior.
/// States: Standby -> AggressiveChase -> WaitingAtSafeZone -> Watching -> AggressiveChase (loop)
/// Mask is hidden during chase/waiting, visible during watching.
/// </summary>
public class KuchisakeOnnaController : MonoBehaviour
{
    #region Constants
    private const float CATCH_PLAYER_DISTANCE = 2f;
    private const float CHASE_LOG_INTERVAL = 2f;

    // Animation distance states
    private const int ANIM_IDLE = 0;
    private const int ANIM_WALKING = 1;
    private const int ANIM_RUNNING = 2;
    #endregion

    #region Serialized Fields
    [Header("Core References")]
    [SerializeField] private Transform player;
    [SerializeField] private VideoTrigger videoTrigger;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [Tooltip("How many seconds to wait at safe zone edge before switching to watching mode")]
    [SerializeField] private float safeZoneWaitTime = 5f;
    [Tooltip("Distance to stop from player to prevent pushing (matches attack range)")]
    [SerializeField] private float attackRange = 2f;
    [Tooltip("Time between attacks in seconds")]
    [SerializeField] private float attackCooldown = 1f;
    [Tooltip("Damage dealt per attack (set to 100+ for instant kill)")]
    [SerializeField] private int attackDamage = 100;

    [Header("Visual Effects")]
    [SerializeField] private GameObject maskObject;
    [SerializeField] private Material normalFaceMaterial;
    [SerializeField] private Material slitMouthMaterial;
    [SerializeField] private Renderer faceRenderer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string distanceParameter = "distanceFromPlayer";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathScreamClip;
    [SerializeField] private AudioClip scissorsSnipSound;

    [Header("Patrol Settings")]
    [Tooltip("Patrol waypoints. If empty, will auto-generate waypoints in radius.")]
    [SerializeField] private Transform[] patrolWaypoints;
    [Tooltip("How long to watch player in safe zone before starting patrol")]
    [SerializeField] private float watchingTimeout = 10f;
    [Tooltip("Speed while patrolling (slower than chase speed)")]
    [SerializeField] private float patrolSpeed = 2f;
    [Tooltip("How long to wait at each patrol waypoint before moving to next")]
    [SerializeField] private float patrolWaitTime = 3f;
    [Tooltip("If no waypoints assigned, generate patrol points in this radius")]
    [SerializeField] private float autoPatrolRadius = 20f;
    [Tooltip("Number of waypoints to auto-generate if none assigned")]
    [SerializeField] private int autoWaypointCount = 4;
    #endregion

    #region Private Fields
    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Standby;
    private EnemyState lastLoggedState = EnemyState.Standby;
    private AnimationState currentAnimationState = AnimationState.Idle;

    private bool chaseActivated = false;
    private float safeZoneWaitTimer = 0f;
    private float chaseLogTimer = 0f;

    // Patrol tracking
    private int currentPatrolWaypointIndex = -1;
    private float patrolWaitTimer = 0f;
    private float watchingTimer = 0f;
    private System.Collections.Generic.List<int> unusedPatrolWaypoints = new System.Collections.Generic.List<int>();
    private System.Collections.Generic.List<Vector3> generatedPatrolPoints = new System.Collections.Generic.List<Vector3>();
    private bool usingGeneratedWaypoints = false;

    // Attack tracking
    private float lastAttackTime = 0f;
    private bool isWithinAttackRange = false;
    private bool isMovementPaused = false;

    // Ending behavior flags
    private bool ignoreSafeZones = false;
    #endregion

    #region Enums
    public enum EnemyState
    {
        Standby,            // Idle, waiting for video to play
        AggressiveChase,    // Chasing player with mask off
        WaitingAtSafeZone,  // Waiting at safe zone edge
        Watching,           // Standing still, monitoring player
        Patrol              // Walking between waypoints with mask on
    }

    public enum AnimationState
    {
        Idle,
        Walking,
        Running
    }
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // Get NavMesh Agent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"[KuchisakeOnna] {gameObject.name}: NavMeshAgent component missing!");
            enabled = false;
            return;
        }

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[KuchisakeOnna] Player found: {player.name}");
            }
            else
            {
                Debug.LogError($"[KuchisakeOnna] Player not found!");
            }
        }

        // Validate video trigger
        if (videoTrigger == null)
        {
            Debug.LogError($"[KuchisakeOnna] VIDEO TRIGGER NOT ASSIGNED! Chase will never activate.");
        }
        else
        {
            Debug.Log($"[KuchisakeOnna] Video trigger assigned: {videoTrigger.gameObject.name}");
        }

        // Start in standby mode - disable agent, wear mask, idle animation
        currentState = EnemyState.Standby;
        agent.enabled = false; // INTENTIONAL: Agent disabled until video plays
        RestoreMask();
        ChangeAnimationState(AnimationState.Idle);

        Debug.Log($"[KuchisakeOnna] Initialized in STANDBY mode. NavMeshAgent DISABLED (intentional - will enable after video).");
    }

    void Update()
    {
        if (player == null) return;

        // CRITICAL: Don't do ANYTHING when movement is paused
        if (isMovementPaused)
        {
            Debug.Log($"[KuchisakeOnna] Update() BLOCKED - isMovementPaused = TRUE");
            return;
        }

        // Monitor video trigger
        if (!chaseActivated)
        {
            MonitorVideoTrigger();
        }

        // Log state changes
        if (currentState != lastLoggedState)
        {
            Debug.Log($"[KuchisakeOnna] *** STATE CHANGED: {lastLoggedState} -> {currentState} ***");
            lastLoggedState = currentState;
        }

        // State machine
        switch (currentState)
        {
            case EnemyState.Standby:
                HandleStandby();
                break;

            case EnemyState.AggressiveChase:
                HandleAggressiveChase();
                break;

            case EnemyState.WaitingAtSafeZone:
                HandleWaitingAtSafeZone();
                break;

            case EnemyState.Watching:
                HandleWatching();
                break;

            case EnemyState.Patrol:
                HandlePatrol();
                break;
        }
    }
    #endregion

    #region State Handlers
    /// <summary>
    /// Standby state - completely idle, waiting for video to trigger.
    /// </summary>
    void HandleStandby()
    {
        // Do nothing - just wait for video
        ChangeAnimationState(AnimationState.Idle);
    }

    /// <summary>
    /// Aggressive chase - relentlessly pursues player with mask off.
    /// Stops at attack range to prevent pushing, attacks on cooldown.
    /// </summary>
    void HandleAggressiveChase()
    {
        Debug.Log($"[KuchisakeOnna] >>> HandleAggressiveChase() called");

        // Validate agent
        bool agentValid = ValidateAgent();
        Debug.Log($"[KuchisakeOnna] ValidateAgent result: {agentValid}");
        if (!agentValid) return;

        // Don't chase if movement is paused (after attack)
        if (isMovementPaused)
        {
            return;
        }

        // Check if player entered safe zone
        bool playerInSafe = IsPlayerInSafeZone();
        Debug.Log($"[KuchisakeOnna] IsPlayerInSafeZone result: {playerInSafe}");

        if (playerInSafe)
        {
            Debug.Log($"[KuchisakeOnna] Player entered safe zone. Waiting at edge...");
            currentState = EnemyState.WaitingAtSafeZone;
            safeZoneWaitTimer = safeZoneWaitTime;
            agent.isStopped = true;
            isWithinAttackRange = false;
            ChangeAnimationState(AnimationState.Idle);
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if within attack range
        if (distanceToPlayer <= attackRange)
        {
            // Stop moving to prevent pushing
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                Debug.Log($"[KuchisakeOnna] Stopped at attack range ({attackRange}m)");
            }

            isWithinAttackRange = true;
            ChangeAnimationState(AnimationState.Idle);

            // Attack on cooldown
            AttackPlayer();
        }
        else
        {
            // Resume chase if player moved away
            if (agent.isStopped)
            {
                agent.isStopped = false;
                Debug.Log($"[KuchisakeOnna] Player moved away, resuming chase");
            }

            isWithinAttackRange = false;
            ChangeAnimationState(AnimationState.Running);

            // Set destination to player
            agent.SetDestination(player.position);
            Debug.Log($"[KuchisakeOnna] >>> agent.SetDestination() called at line 287 - Player pos: {player.position}");
        }

        // Periodic logging
        chaseLogTimer += Time.deltaTime;
        if (chaseLogTimer >= CHASE_LOG_INTERVAL)
        {
            Debug.Log($"[KuchisakeOnna] Chasing - Distance: {distanceToPlayer:F2}m | In Attack Range: {isWithinAttackRange} | Velocity: {agent.velocity.magnitude:F2}");
            chaseLogTimer = 0f;
        }
    }

    /// <summary>
    /// Waiting at safe zone edge - player is in unwalkable area, timer counts down.
    /// </summary>
    void HandleWaitingAtSafeZone()
    {
        ChangeAnimationState(AnimationState.Idle);

        // Check if player left safe zone
        if (!IsPlayerInSafeZone())
        {
            Debug.Log($"[KuchisakeOnna] Player left safe zone! Resuming chase...");
            currentState = EnemyState.AggressiveChase;
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            RemoveMask();
            return;
        }

        // Count down timer
        safeZoneWaitTimer -= Time.deltaTime;

        if (safeZoneWaitTimer <= 0)
        {
            Debug.Log($"[KuchisakeOnna] Wait timer expired. Switching to WATCHING mode...");
            currentState = EnemyState.Watching;
            RestoreMask(); // Put mask back on when watching
        }
    }

    /// <summary>
    /// Watching mode - stands still at current position, monitors for player leaving safe zone.
    /// After timeout, transitions to patrol mode.
    /// </summary>
    void HandleWatching()
    {
        ChangeAnimationState(AnimationState.Idle);

        // Increment watching timer
        watchingTimer += Time.deltaTime;

        // Check if watching timeout expired -> start patrol
        if (watchingTimer >= watchingTimeout)
        {
            Debug.Log($"[KuchisakeOnna] Watching timeout expired ({watchingTimeout}s). Starting patrol...");
            TransitionToPatrol();
            return;
        }

        // Continuously check if player left safe zone
        if (!IsPlayerInSafeZone())
        {
            Debug.Log($"[KuchisakeOnna] Player left safe zone during watching! Resuming chase...");
            watchingTimer = 0f; // Reset timer

            // Resume chase
            currentState = EnemyState.AggressiveChase;

            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
            }

            RemoveMask();
        }
    }

    /// <summary>
    /// Patrol mode - walks between waypoints with mask on, looking for player to exit safe zone.
    /// </summary>
    void HandlePatrol()
    {
        if (isMovementPaused) return;

        // Always check if player left safe zone
        if (!IsPlayerInSafeZone())
        {
            Debug.Log($"[KuchisakeOnna] Player left safe zone during patrol! Resuming chase...");
            currentState = EnemyState.AggressiveChase;
            watchingTimer = 0f; // Reset timer
            patrolWaitTimer = 0f; // Reset timer

            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed = chaseSpeed;
            }

            RemoveMask();
            return;
        }

        // Validate agent
        if (!ValidateAgent()) return;

        // If no waypoints available, stay idle in place
        if (GetPatrolWaypointCount() == 0)
        {
            ChangeAnimationState(AnimationState.Idle);
            return;
        }

        // Set walking animation if moving
        ChangeAnimationState(AnimationState.Walking);

        // Check if reached waypoint
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            // Wait at waypoint
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                // Move to next waypoint
                GoToNextPatrolWaypoint();
                patrolWaitTimer = 0f;
            }
            else
            {
                // Idle while waiting
                ChangeAnimationState(AnimationState.Idle);
            }
        }
    }
    #endregion

    #region Video Trigger
    /// <summary>
    /// Monitors video trigger to detect when video has played.
    /// </summary>
    void MonitorVideoTrigger()
    {
        if (chaseActivated) return;
        if (videoTrigger == null) return;

        if (videoTrigger.hasPlayed)
        {
            Debug.Log($"[KuchisakeOnna] VIDEO PLAYED! Activating aggressive chase...");
            ActivateChase();
        }
    }

    /// <summary>
    /// Activates aggressive chase mode after video plays.
    /// </summary>
    void ActivateChase()
    {
        Debug.Log($"[KuchisakeOnna] ========== ACTIVATING CHASE ==========");

        chaseActivated = true;

        // Enable NavMesh agent
        if (agent != null)
        {
            Debug.Log($"[KuchisakeOnna] RE-ENABLING NavMeshAgent (was disabled during standby)...");
            agent.enabled = true;

            // Validate agent is on NavMesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"[KuchisakeOnna] Agent is NOT on NavMesh at position: {transform.position}");

                // Try to find nearest NavMesh point
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    Debug.LogWarning($"[KuchisakeOnna] Nearest NavMesh: {hit.position} (distance: {Vector3.Distance(transform.position, hit.position):F2}m)");
                    Debug.LogWarning($"[KuchisakeOnna] SOLUTION: Move enemy GameObject to NavMesh location.");
                }
                return;
            }

            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange; // Stop at attack range to prevent pushing
            agent.isStopped = false;
            Debug.Log($"[KuchisakeOnna] Agent enabled. Speed: {chaseSpeed} | Stopping Distance: {attackRange} | On NavMesh: {agent.isOnNavMesh}");
        }

        // Remove mask
        RemoveMask();

        // Start chase
        currentState = EnemyState.AggressiveChase;

        Debug.Log($"[KuchisakeOnna] Chase activated! State: {currentState}");
        Debug.Log($"[KuchisakeOnna] Player position: {player.position} | Enemy position: {transform.position}");
        Debug.Log($"[KuchisakeOnna] ========================================");
    }
    #endregion

    #region Safe Zone Detection
    /// <summary>
    /// Checks if player is in a safe zone using trigger-based detection.
    /// Returns false if ignoreSafeZones flag is set (bad ending behavior).
    /// </summary>
    bool IsPlayerInSafeZone()
    {
        // If we're ignoring safe zones (bad ending), player is never safe
        if (ignoreSafeZones)
        {
            return false;
        }

        return SafeZoneTrigger.IsPlayerInAnySafeZone;
    }
    #endregion

    #region Patrol Methods
    /// <summary>
    /// Transitions from Watching to Patrol mode
    /// </summary>
    void TransitionToPatrol()
    {
        currentState = EnemyState.Patrol;
        watchingTimer = 0f;

        // Initialize patrol waypoints
        InitializePatrolWaypoints();

        // Configure agent for patrol
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;
        }

        // Ensure mask is on
        RestoreMask();

        // Go to first waypoint
        GoToNextPatrolWaypoint();
    }

    /// <summary>
    /// Initialize patrol waypoint system
    /// </summary>
    void InitializePatrolWaypoints()
    {
        // Check if manual waypoints assigned
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            // Count valid waypoints
            int validCount = 0;
            foreach (var wp in patrolWaypoints)
            {
                if (wp != null) validCount++;
            }

            if (validCount > 0)
            {
                usingGeneratedWaypoints = false;
                ResetUnusedPatrolWaypoints();
                Debug.Log($"[KuchisakeOnna] Using {validCount} manual patrol waypoints");
                return;
            }
        }

        // Generate waypoints automatically
        GeneratePatrolWaypoints();
    }

    /// <summary>
    /// Generate patrol waypoints in circle pattern around current position
    /// </summary>
    void GeneratePatrolWaypoints()
    {
        generatedPatrolPoints.Clear();
        Vector3 center = transform.position;

        for (int i = 0; i < autoWaypointCount; i++)
        {
            float angle = (360f / autoWaypointCount) * i;
            float distance = Random.Range(autoPatrolRadius * 0.6f, autoPatrolRadius);

            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 targetPos = center + direction * distance;

            // Sample NavMesh to find valid position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, autoPatrolRadius, NavMesh.AllAreas))
            {
                generatedPatrolPoints.Add(hit.position);
            }
        }

        usingGeneratedWaypoints = true;
        ResetUnusedPatrolWaypoints();
        Debug.Log($"[KuchisakeOnna] Generated {generatedPatrolPoints.Count} patrol waypoints");
    }

    /// <summary>
    /// Move to next patrol waypoint using random selection
    /// </summary>
    void GoToNextPatrolWaypoint()
    {
        int waypointCount = GetPatrolWaypointCount();

        if (waypointCount == 0)
        {
            Debug.LogWarning($"[KuchisakeOnna] No patrol waypoints available! Staying in place and monitoring for player leaving safe zone.");

            // Stop the agent in place but keep patrol state active
            // HandlePatrol() will continue to check if player leaves safe zone
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            return;
        }

        // Reset list if empty (all waypoints visited)
        if (unusedPatrolWaypoints.Count == 0)
        {
            ResetUnusedPatrolWaypoints();
        }

        // Prevent immediate repeat of last waypoint if possible
        if (currentPatrolWaypointIndex != -1 && unusedPatrolWaypoints.Count > 1)
        {
            unusedPatrolWaypoints.Remove(currentPatrolWaypointIndex);
        }

        // Pick random unused waypoint
        if (unusedPatrolWaypoints.Count > 0)
        {
            int randomIndex = Random.Range(0, unusedPatrolWaypoints.Count);
            currentPatrolWaypointIndex = unusedPatrolWaypoints[randomIndex];
            unusedPatrolWaypoints.RemoveAt(randomIndex);
        }

        Vector3 destination = GetPatrolWaypointPosition(currentPatrolWaypointIndex);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
            Debug.Log($"[KuchisakeOnna] Moving to patrol waypoint {currentPatrolWaypointIndex} at {destination}");
        }
    }

    /// <summary>
    /// Reset the list of unused waypoints for random patrol
    /// </summary>
    void ResetUnusedPatrolWaypoints()
    {
        unusedPatrolWaypoints.Clear();
        int count = GetPatrolWaypointCount();

        for (int i = 0; i < count; i++)
        {
            unusedPatrolWaypoints.Add(i);
        }
    }

    /// <summary>
    /// Get waypoint position by index
    /// </summary>
    Vector3 GetPatrolWaypointPosition(int index)
    {
        if (usingGeneratedWaypoints)
        {
            if (index >= 0 && index < generatedPatrolPoints.Count)
                return generatedPatrolPoints[index];
        }
        else
        {
            if (patrolWaypoints != null && index >= 0 && index < patrolWaypoints.Length)
            {
                if (patrolWaypoints[index] != null)
                    return patrolWaypoints[index].position;
            }
        }

        Debug.LogWarning($"[KuchisakeOnna] Invalid waypoint index: {index}");
        return transform.position;
    }

    /// <summary>
    /// Get total number of patrol waypoints
    /// </summary>
    int GetPatrolWaypointCount()
    {
        if (usingGeneratedWaypoints)
            return generatedPatrolPoints.Count;

        if (patrolWaypoints != null)
            return patrolWaypoints.Length;

        return 0;
    }
    #endregion

    #region Player Interaction
    /// <summary>
    /// Attacks the player with cooldown system.
    /// </summary>
    void AttackPlayer()
    {
        if (isMovementPaused) return;

        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        // Play attack sound
        PlaySound(scissorsSnipSound);

        // Deal damage to player
        if (player != null)
        {
            PlayerHealthSystem playerHealth = player.GetComponent<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(attackDamage);
                Debug.Log($"[KuchisakeOnna] Attacked player for {attackDamage} damage. Player health: {playerHealth.CurrentHealth}");

                // Check if player died
                if (playerHealth.CurrentHealth <= 0)
                {
                    Debug.Log($"[KuchisakeOnna] Player killed!");
                    PlaySound(deathScreamClip);

                    // Stop chasing
                    if (agent != null)
                    {
                        agent.isStopped = true;
                    }
                }
                else
                {
                    // Player survived - pause movement to give them a chance to escape
                    if (!isMovementPaused)
                    {
                        // CRITICAL: Set pause flag IMMEDIATELY before starting coroutine
                        isMovementPaused = true;

                        // IMMEDIATELY stop the agent
                        if (agent != null && agent.enabled && agent.isOnNavMesh)
                        {
                            agent.isStopped = true;
                            agent.ResetPath();
                            agent.velocity = Vector3.zero;
                            Debug.Log($"[KuchisakeOnna] Agent STOPPED immediately in AttackPlayer()");
                        }

                        StartCoroutine(PauseMovementAfterAttack(attackCooldown));
                        Debug.Log($"[KuchisakeOnna] isMovementPaused set to TRUE immediately");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[KuchisakeOnna] PlayerHealthSystem component not found!");
            }
        }
    }

    /// <summary>
    /// Called when enemy catches the player (legacy method - now uses AttackPlayer with cooldown).
    /// </summary>
    void CatchPlayer()
    {
        Debug.Log($"[KuchisakeOnna] Player caught!");

        // Play death effects
        PlaySound(deathScreamClip);
        PlaySound(scissorsSnipSound);

        // Kill player
        if (player != null)
        {
            PlayerHealthSystem playerHealth = player.GetComponent<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.SetHealth(0); // Instantly kill player
            }
            else
            {
                Debug.LogWarning($"[KuchisakeOnna] PlayerHealthSystem component not found!");
            }
        }

        // Stop chasing
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }
    #endregion

    #region Mask Management
    /// <summary>
    /// Removes mask to reveal slit-mouth face.
    /// </summary>
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

        Debug.Log($"[KuchisakeOnna] Mask removed (slit-mouth revealed)");
    }

    /// <summary>
    /// Restores mask to hide slit-mouth face.
    /// </summary>
    void RestoreMask()
    {
        if (maskObject != null)
        {
            maskObject.SetActive(true);
        }

        if (faceRenderer != null && normalFaceMaterial != null)
        {
            faceRenderer.material = normalFaceMaterial;
        }

        Debug.Log($"[KuchisakeOnna] Mask restored");
    }
    #endregion

    #region Animation
    /// <summary>
    /// Changes animation state and updates animator parameters.
    /// </summary>
    void ChangeAnimationState(AnimationState newState)
    {
        if (currentAnimationState == newState) return;

        currentAnimationState = newState;
        UpdateAnimator();
    }

    /// <summary>
    /// Updates animator parameters based on current animation state.
    /// </summary>
    void UpdateAnimator()
    {
        if (animator == null || string.IsNullOrEmpty(distanceParameter)) return;

        int distanceValue = ANIM_IDLE;

        switch (currentAnimationState)
        {
            case AnimationState.Idle:
                distanceValue = ANIM_IDLE;
                break;

            case AnimationState.Walking:
                distanceValue = ANIM_WALKING;
                break;

            case AnimationState.Running:
                distanceValue = ANIM_RUNNING;
                break;
        }

        animator.SetInteger(distanceParameter, distanceValue);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Validates that NavMesh agent is ready for movement.
    /// </summary>
    bool ValidateAgent()
    {
        if (agent == null)
        {
            Debug.LogError($"[KuchisakeOnna] Agent is NULL!");
            return false;
        }

        if (!agent.enabled)
        {
            Debug.LogError($"[KuchisakeOnna] Agent is DISABLED!");
            return false;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"[KuchisakeOnna] Agent is NOT on NavMesh!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Plays audio clip safely.
    /// </summary>
    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// Pauses enemy movement for specified duration after attacking player.
    /// Gives player a chance to escape.
    /// </summary>
    private System.Collections.IEnumerator PauseMovementAfterAttack(float duration)
    {
        // Note: isMovementPaused is already set to true in AttackPlayer() before this coroutine starts
        Debug.Log($"[KuchisakeOnna] Pausing movement for {duration} seconds after attack");

        // Wait for cooldown duration
        yield return new WaitForSeconds(duration);

        // Resume movement only if still in aggressive chase state
        if (agent != null && agent.enabled && agent.isOnNavMesh && currentState == EnemyState.AggressiveChase)
        {
            agent.isStopped = false;
            Debug.Log($"[KuchisakeOnna] Resuming chase after {duration}s pause");
        }

        isMovementPaused = false;
    }
    #endregion

    #region Ending Behaviors
    /// <summary>
    /// Good ending behavior: Makes the enemy disappear (instant disable).
    /// Called when sequential torch puzzle is completed in GOOD ENDING scene.
    /// </summary>
    public void DisappearEnemy()
    {
        Debug.Log("[KuchisakeOnna] DisappearEnemy() called - Good ending behavior activated");

        // Stop all current behavior
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Debug.Log("[KuchisakeOnna] Enemy disabled. Good ending complete.");

        // Immediately disable the GameObject
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Bad ending behavior: Makes the enemy more aggressive and dangerous.
    /// Called when sequential torch puzzle is completed in BAD ENDING scene.
    /// </summary>
    public void EnrageEnemy()
    {
        Debug.Log("[KuchisakeOnna] EnrageEnemy() called - Bad ending behavior activated");
        Debug.Log("[KuchisakeOnna] 💀 BAD ENDING MODE: Enemy is now ENRAGED! 💀");

        // Boost movement speed
        float oldSpeed = chaseSpeed;
        chaseSpeed = 6f;
        if (agent != null)
        {
            agent.speed = chaseSpeed;
        }
        Debug.Log($"[KuchisakeOnna] Chase speed boosted: {oldSpeed} → {chaseSpeed}");

        // Reduce attack cooldown for more frequent attacks
        float oldCooldown = attackCooldown;
        attackCooldown = 0.5f;
        Debug.Log($"[KuchisakeOnna] Attack cooldown reduced: {oldCooldown}s → {attackCooldown}s");

        // Enable safe zone ignore mode
        ignoreSafeZones = true;
        Debug.Log("[KuchisakeOnna] Safe zones are now DISABLED - player cannot hide!");

        // Increase damage if needed (optional - currently instant kill already)
        Debug.Log($"[KuchisakeOnna] Attack damage: {attackDamage} (instant kill)");

        Debug.Log("[KuchisakeOnna] ========== ENRAGE COMPLETE - PLAYER BEWARE! ==========");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Called by EnemyDamageController to sync pause state
    /// </summary>
    public void SetMovementPausedExternal(bool paused)
    {
        isMovementPaused = paused;
        if (paused && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            Debug.Log($"[KuchisakeOnna] Movement paused by EnemyDamageController");
        }
        else if (!paused && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            Debug.Log($"[KuchisakeOnna] Movement resumed by EnemyDamageController");
        }
    }

    /// <summary>
    /// Resets enemy to initial state.
    /// </summary>
    public void ResetEnemy()
    {
        Debug.Log($"[KuchisakeOnna] Resetting enemy...");

        chaseActivated = false;
        currentState = EnemyState.Standby;
        safeZoneWaitTimer = 0f;
        chaseLogTimer = 0f;

        // Reset attack tracking
        lastAttackTime = 0f;
        isWithinAttackRange = false;

        if (agent != null)
        {
            agent.enabled = false;
            agent.isStopped = false;
        }

        RestoreMask();
        ChangeAnimationState(AnimationState.Idle);
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        // Draw catch distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, CATCH_PLAYER_DISTANCE);

        // Safe zone visualization is now handled by SafeZoneTrigger components
        // (Green = player inside, Yellow = empty)
    }
    #endregion
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simplified Kuchisake-onna controller with video-triggered chase behavior.
/// States: Standby -> AggressiveChase -> WaitingAtSafeZone -> Watching -> AggressiveChase (loop)
/// Mask is hidden during chase/waiting, visible during watching.
/// </summary>
public class KuchisakeOnnaController : MonoBehaviour
{
    #region Constants
    private const float CATCH_PLAYER_DISTANCE = 2f;
    private const float SAFE_ZONE_CHECK_RADIUS = 10f; // Large radius to find NavMesh
    private const float SAFE_ZONE_DISTANCE_THRESHOLD = 2f; // Player must be 2m+ from NavMesh to be "safe"
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
    #endregion

    #region Private Fields
    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Standby;
    private EnemyState lastLoggedState = EnemyState.Standby;
    private AnimationState currentAnimationState = AnimationState.Idle;

    private bool chaseActivated = false;
    private float safeZoneWaitTimer = 0f;
    private float chaseLogTimer = 0f;
    #endregion

    #region Enums
    public enum EnemyState
    {
        Standby,            // Idle, waiting for video to play
        AggressiveChase,    // Chasing player with mask off
        WaitingAtSafeZone,  // Waiting at safe zone edge
        Watching            // Standing still, monitoring player
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
    /// </summary>
    void HandleAggressiveChase()
    {
        Debug.Log($"[KuchisakeOnna] >>> HandleAggressiveChase() called");

        // Validate agent
        bool agentValid = ValidateAgent();
        Debug.Log($"[KuchisakeOnna] ValidateAgent result: {agentValid}");
        if (!agentValid) return;

        // Check if player entered safe zone
        bool playerInSafe = IsPlayerInSafeZone();
        Debug.Log($"[KuchisakeOnna] IsPlayerInSafeZone result: {playerInSafe}");

        if (playerInSafe)
        {
            Debug.Log($"[KuchisakeOnna] Player entered safe zone. Waiting at edge...");
            currentState = EnemyState.WaitingAtSafeZone;
            safeZoneWaitTimer = safeZoneWaitTime;
            agent.isStopped = true;
            ChangeAnimationState(AnimationState.Idle);
            return;
        }

        // Chase behavior
        ChangeAnimationState(AnimationState.Running);

        // Update destination to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > 1f)
        {
            agent.SetDestination(player.position);
        }

        // Periodic logging
        chaseLogTimer += Time.deltaTime;
        if (chaseLogTimer >= CHASE_LOG_INTERVAL)
        {
            Debug.Log($"[KuchisakeOnna] Chasing - Distance: {distanceToPlayer:F2}m | Velocity: {agent.velocity.magnitude:F2}");
            chaseLogTimer = 0f;
        }

        // Check if caught player
        if (distanceToPlayer < CATCH_PLAYER_DISTANCE)
        {
            CatchPlayer();
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
    /// </summary>
    void HandleWatching()
    {
        ChangeAnimationState(AnimationState.Idle);

        // Continuously check if player left safe zone
        if (!IsPlayerInSafeZone())
        {
            Debug.Log($"[KuchisakeOnna] Player left safe zone during watching! Resuming chase...");

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
            agent.isStopped = false;
            Debug.Log($"[KuchisakeOnna] Agent enabled. Speed: {chaseSpeed} | On NavMesh: {agent.isOnNavMesh}");
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
    /// Checks if player is in a safe zone (unwalkable area, off NavMesh).
    /// </summary>
    bool IsPlayerInSafeZone()
    {
        if (player == null)
        {
            Debug.LogWarning($"[KuchisakeOnna] IsPlayerInSafeZone: Player is NULL!");
            return false;
        }

        // Check if player position is near NavMesh
        NavMeshHit hit;
        bool foundNavMesh = NavMesh.SamplePosition(player.position, out hit, SAFE_ZONE_CHECK_RADIUS, NavMesh.AllAreas);

        // Calculate distance to nearest NavMesh point
        float distanceToNavMesh = foundNavMesh ? Vector3.Distance(player.position, hit.position) : 999f;

        // Smart detection: Player is in safe zone if:
        // 1. No NavMesh found within search radius, OR
        // 2. NavMesh found but player is far from it (> threshold)
        bool inSafeZone = !foundNavMesh || distanceToNavMesh > SAFE_ZONE_DISTANCE_THRESHOLD;

        Debug.Log($"[KuchisakeOnna] Smart Safe Zone Check:");
        Debug.Log($"  Player position: {player.position}");
        Debug.Log($"  Search radius: {SAFE_ZONE_CHECK_RADIUS}m");
        Debug.Log($"  NavMesh found: {foundNavMesh}");
        if (foundNavMesh)
        {
            Debug.Log($"  Nearest NavMesh point: {hit.position}");
            Debug.Log($"  Distance to NavMesh: {distanceToNavMesh:F2}m");
            Debug.Log($"  Threshold: {SAFE_ZONE_DISTANCE_THRESHOLD}m");
        }
        Debug.Log($"  RESULT: Player is {(inSafeZone ? "IN SAFE ZONE (chase stops)" : "ON NAVMESH (chase continues)")}");

        return inSafeZone;
    }
    #endregion

    #region Player Interaction
    /// <summary>
    /// Called when enemy catches the player.
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
    #endregion

    #region Public Methods
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

        // Draw safe zone detection at player position
        if (player != null)
        {
            // Green sphere = safe zone threshold (2m from NavMesh)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, SAFE_ZONE_DISTANCE_THRESHOLD);

            // Cyan sphere = search radius (10m to find NavMesh)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, SAFE_ZONE_CHECK_RADIUS);
        }
    }
    #endregion
}
